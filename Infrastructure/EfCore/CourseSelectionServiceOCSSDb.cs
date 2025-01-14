using Microsoft.EntityFrameworkCore;

namespace CourseSelectionService01_OCSS.Infrastructure.EfCore
{
    public class CourseSelectionServiceOCSSDb:DbContext
    {
        public CourseSelectionServiceOCSSDb()
        {

        }

        public CourseSelectionServiceOCSSDb(DbContextOptions<CourseSelectionServiceOCSSDb> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
