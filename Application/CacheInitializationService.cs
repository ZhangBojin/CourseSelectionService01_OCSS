using CourseSelectionService01_OCSS.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CourseSelectionService01_OCSS.Application;

public class CacheInitializationService(IConnectionMultiplexer connectionMultiplexer, CourseServicesDbContext context)
{
    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;
    private readonly CourseServicesDbContext _context = context;

    public async Task Init()
    {
        var courseAvailabilities = await _context.CourseAvailabilities.ToListAsync();

        var redis=_connectionMultiplexer.GetDatabase(1);

        await redis.ExecuteAsync("FLUSHDB");

        foreach (var data in courseAvailabilities)
        {
            var hashEntries = new HashEntry[]
            {
                new HashEntry("CurrentNum", data.CurrentNum),
                new HashEntry("TotalNum", data.TotalNum)
            };
            await redis.HashSetAsync(data.CoursesId.ToString(), hashEntries);
        }
    }
}