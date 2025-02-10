using System;
using System.Collections.Generic;
using CourseSelectionService01_OCSS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseSelectionService01_OCSS.Infrastructure.EfCore;

public partial class CourseSelectionServiceOcssContext : DbContext
{
    public CourseSelectionServiceOcssContext()
    {
    }

    public CourseSelectionServiceOcssContext(DbContextOptions<CourseSelectionServiceOcssContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
