using CourseSelectionService01_OCSS.Domain.IRepositories;
using CourseSelectionService01_OCSS.Infrastructure.RabbitMq;
using CourseSelectionService01_OCSS.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace CourseSelectionService01_OCSS.Application
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CourseSelectionController(IConnectionMultiplexer connectionMultiplexer, RabbitMqProducer rabbitMqProducer, IEnrollmentRepository enrollmentRepository, ICourseRepository courseRepository) : ControllerBase
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;
        private readonly RabbitMqProducer _rabbitMqProducer = rabbitMqProducer;

        private readonly ICourseRepository _courseRepository = courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository = enrollmentRepository;

        [Authorize(Roles = "学生")]
        [HttpPost]
        public async Task<IActionResult> SelectConfirm(string id)
        {
            var userClaims = User.Claims;
            var userId = userClaims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var redis = _connectionMultiplexer.GetDatabase(1);
            var lockKey = $"lock:{id}"; // 锁的唯一标识，防止不同的选课项同时被加锁
            var lockTimeout = TimeSpan.FromSeconds(10); // 锁的过期时间，避免死锁 10秒
            const int maxRetries = 5; // 最大重试次数
            var retryDelay = TimeSpan.FromMilliseconds(500); // 每次重试的延迟时间 半秒

            var retryCount = 0;
            var lockAcquired = false;

            var userCoursesKey = $"user:{userId}:courses";

            // 检查是否已选任意课程
            var selectedCourses = await redis.HashLengthAsync(userCoursesKey);
            if (selectedCourses > 0)
            {
                return StatusCode(409, "你已选择一门课程，无法再次选课。");
            }

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

                await redis.HashSetAsync(userCoursesKey, id, true);

                _rabbitMqProducer.SelectConfirmMq(Convert.ToInt32(userId), Convert.ToInt32(id));

                return Ok("选课成功！");
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

        [Authorize(Roles = "学生")]
        [HttpPost]
        public async Task<IActionResult> GetMyCourse()
        {
            var userClaims = User.Claims;
            var userId = userClaims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var data = await _courseRepository.GetMyCourseInfo(await _enrollmentRepository.GetMyCourseId(Convert.ToInt32(userId)));

            return Ok(data);
        }
    }
}

