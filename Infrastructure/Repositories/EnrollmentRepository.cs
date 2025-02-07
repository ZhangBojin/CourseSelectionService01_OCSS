using CourseSelectionService01_OCSS.Domain.IRepositories;
using CourseSelectionService01_OCSS.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;

namespace CourseSelectionService01_OCSS.Infrastructure.Repositories;

public class EnrollmentRepository(CourseSelectionServiceOcssContext context) : IEnrollmentRepository
{
    private readonly CourseSelectionServiceOcssContext _context = context;

    public async Task<List<int>> GetMyCourseId(int userId)
    {
       return   await _context.Enrollments.Where(e => e.UserId == userId).Select(e=>e.CoursesId).ToListAsync();
    }
}