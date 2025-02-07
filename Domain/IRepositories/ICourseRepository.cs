namespace CourseSelectionService01_OCSS.Domain.IRepositories;

public interface ICourseRepository
{
    Task<dynamic> GetMyCourseInfo(List<int> CoursesId);
}