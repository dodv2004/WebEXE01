using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{
    public class ProjectEXE01Context : DbContext
    {
        public ProjectEXE01Context(DbContextOptions<ProjectEXE01Context> options)
            : base(options)
        {
        }

        public DbSet<ReportCheck> ReportChecks { get; set; }
        public DbSet<Reporter> Reporters { get; set; }
        public DbSet<ReporterReportCheck> ReporterReportChecks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình khóa chính kép cho bảng trung gian
            modelBuilder.Entity<ReporterReportCheck>()
                .HasKey(rr => new { rr.ReporterId, rr.ReportCheckId });

            modelBuilder.Entity<ReporterReportCheck>()
                .HasOne(rr => rr.Reporter)
                .WithMany(r => r.ReporterReportChecks)
                .HasForeignKey(rr => rr.ReporterId);

            modelBuilder.Entity<ReporterReportCheck>()
                .HasOne(rr => rr.ReportCheck)
                .WithMany(rc => rc.ReporterReportChecks)
                .HasForeignKey(rr => rr.ReportCheckId);
        }
    }
}
