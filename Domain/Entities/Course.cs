using System;
using System.Collections.Generic;

namespace CourseSelectionService01_OCSS.Domain.Entities;

public partial class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? ClassTime { get; set; }

    public string? Classroom { get; set; }

    public string? Description { get; set; }

    public float? Credits { get; set; }

    public string? Type { get; set; }

    public int? MaxStudents { get; set; }

    public string? TeachingMethod { get; set; }

    public int? TeacherId { get; set; }

    public string? TeacherName { get; set; }

    public virtual ICollection<CourseAvailability> CourseAvailabilities { get; set; } = new List<CourseAvailability>();
}
