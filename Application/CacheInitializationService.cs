using CourseSelectionService01_OCSS.Domain.IRepositories;
using CourseSelectionService01_OCSS.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CourseSelectionService01_OCSS.Application;

public class CacheInitializationService(IConnectionMultiplexer connectionMultiplexer, CourseServicesDbContext courseServicesDb, CourseSelectionServiceOcssContext courseSelectionServiceOcssDb
    )
{
    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;
    private readonly CourseServicesDbContext _courseServicesDb = courseServicesDb;
    private readonly CourseSelectionServiceOcssContext _courseSelectionServiceOcssDb = courseSelectionServiceOcssDb;
   

    public async Task Init()
    {
        var courseAvailabilities = await _courseServicesDb.CourseAvailabilities.ToListAsync();
        var enrollments = await _courseSelectionServiceOcssDb.Enrollments.ToListAsync();


        var redis =_connectionMultiplexer.GetDatabase(1);

        await redis.ExecuteAsync("FLUSHDB");

        // 并发写入任务列表
        var tasks = new List<Task>();

        // 批量处理 CourseAvailabilities 数据
        tasks.AddRange(courseAvailabilities.Select(data =>
        {
            var hashEntries = new HashEntry[]
            {
                new HashEntry("CurrentNum", data.CurrentNum),
                new HashEntry("TotalNum", data.TotalNum)
            };
            return redis.HashSetAsync(data.CoursesId.ToString(), hashEntries);
        }));

        // 批量处理 Enrollments 数据
        tasks.AddRange(enrollments.Select(data =>
        {
            var userCoursesKey = $"user:{data.UserId}:courses";
            return redis.HashSetAsync(userCoursesKey, data.CoursesId, true);

        }));

        // 等待所有写入任务完成
        await Task.WhenAll(tasks);
    }
}