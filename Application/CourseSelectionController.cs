﻿using CourseSelectionService01_OCSS.Infrastructure.RabbitMq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace CourseSelectionService01_OCSS.Application
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CourseSelectionController(IConnectionMultiplexer connectionMultiplexer, RabbitMqProducer rabbitMqProducer) : ControllerBase
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;
        private readonly RabbitMqProducer _rabbitMqProducer = rabbitMqProducer;

        [HttpPost]
        public async Task<IActionResult> SelectConfirm(string id)
        {
            var redis = _connectionMultiplexer.GetDatabase(1);
            var lockKey = $"lock:{id}"; // 锁的唯一标识，防止不同的选课项同时被加锁
            var lockTimeout = TimeSpan.FromSeconds(10); // 锁的过期时间，避免死锁 10秒
            const int maxRetries = 5; // 最大重试次数
            var retryDelay = TimeSpan.FromMilliseconds(500); // 每次重试的延迟时间 半秒

            var retryCount = 0;
            var lockAcquired = false;

            // 尝试获取锁
            while (retryCount < maxRetries)
            {
                lockAcquired = await redis.StringSetAsync(lockKey, "locked", lockTimeout, When.NotExists);
                if (lockAcquired)
                {
                    break; // 获取锁成功，跳出循环
                }

                retryCount++;
                // 如果未获取到锁，等待一段时间后重试
                await Task.Delay(retryDelay);
            }

            // 如果获取锁失败，返回失败响应
            if (!lockAcquired)
            {
                return StatusCode(409, "选课操作正在进行中，请稍后再试。");
            }

            try
            {
                var currentNum = Convert.ToInt32(await redis.HashGetAsync(id, "CurrentNum"));
                var totalNum = Convert.ToInt32(await redis.HashGetAsync(id, "TotalNum"));

                if (currentNum >= totalNum)
                {
                    return StatusCode(409, "选课人数已满，无法继续选课。");
                }

                // 使用 Redis 原子的 INCR 命令进行自增，增加选课人数
                await redis.HashIncrementAsync(id, "CurrentNum", 1);

                _rabbitMqProducer.SelectConfirmMq(Convert.ToInt32(id));

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "选课操作失败，请稍后再试。");
            }
            finally
            {
                await redis.KeyDeleteAsync(lockKey);
            }
        }
    }
}

