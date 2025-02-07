namespace CourseSelectionService01_OCSS.Domain.IRepositories;

public interface IEnrollmentRepository
{
    Task<List<int>> GetMyCourseId(int userId);
}