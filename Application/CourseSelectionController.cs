using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace CourseSelectionService01_OCSS.Application
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CourseSelectionController(IConnectionMultiplexer connectionMultiplexer) : ControllerBase
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;

        [HttpPost]
        public async Task<IActionResult> SelectConfirm(string id)
        {
            var redis = _connectionMultiplexer.GetDatabase(1);
            var currentNum = Convert.ToInt32(await redis.HashGetAsync(id, "CurrentNum"));
            var totalNum = Convert.ToInt32(await redis.HashGetAsync(id, "TotalNum"));

            if (currentNum == totalNum ) return StatusCode(409, "选课人数已满，无法继续选课。");

            // 使用 Redis 原子的 INCR 命令进行自增
            await redis.HashIncrementAsync(id, "CurrentNum", 1);


            return Ok();
        }
    }
}
