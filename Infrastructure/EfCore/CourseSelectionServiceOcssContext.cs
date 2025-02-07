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
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=37.tcp.cpolar.top,10487;Database=CourseSelectionService_OCSS;uid=sa;pwd=zz;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
