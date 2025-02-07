using CourseSelectionService01_OCSS.Domain.IRepositories;
using CourseSelectionService01_OCSS.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;

namespace CourseSelectionService01_OCSS.Infrastructure.Repositories;

public class CourseRepository(CourseServicesDbContext context) : ICourseRepository
{
    private readonly CourseServicesDbContext _context = context;

    public async Task<dynamic> GetMyCourseInfo(List<int> coursesId)
    {
        return await _context.Courses.Where(c => coursesId.Contains(c.Id)).Select(c=>new
        {
            c.Id,
            c.Name,
        }).ToListAsync();
    }
}