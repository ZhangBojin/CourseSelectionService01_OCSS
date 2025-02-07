using System;
using System.Collections.Generic;

namespace CourseSelectionService01_OCSS.Domain.Entities;

public partial class Enrollment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CoursesId { get; set; }

    public DateTime EnrollmentDate { get; set; }

    public double? Grade { get; set; }

    public bool? IsPass { get; set; }
}
