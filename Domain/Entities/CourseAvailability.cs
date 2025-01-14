using System;
using System.Collections.Generic;

namespace CourseSelectionService01_OCSS.Domain.Entities;

public partial class CourseAvailability
{
    public int Id { get; set; }

    public int CoursesId { get; set; }

    public int CurrentNum { get; set; }

    public int TotalNum { get; set; }

    public virtual Course Courses { get; set; } = null!;
}
