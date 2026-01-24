using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace DAL.Data
{
    public class ProjectEXE01Context : DbContext
    {
        public ProjectEXE01Context(DbContextOptions<ProjectEXE01Context> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ReportCheck> ReportChecks { get; set; }
        public DbSet<Reporter> Reporters { get; set; }
        public DbSet<ReporterReportCheck> ReporterReportChecks { get; set; }

        public DbSet<Appeal> Appeals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----------------------------------------------------------
            // 1. Cấu hình quan hệ N-N (Reporter <-> ReportCheck)
            // ----------------------------------------------------------
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

            // ----------------------------------------------------------
            // 2. Cấu hình quan hệ 1-N (ReportCheck <-> Appeal)
            // ----------------------------------------------------------
            modelBuilder.Entity<Appeal>()
                .HasOne(a => a.ReportCheck)             // Một Appeal liên quan đến một ReportCheck
                .WithMany(rc => rc.Appeals)             // Một ReportCheck có nhiều Appeals
                .HasForeignKey(a => a.ReportCheckId)    // Khóa ngoại là ReportCheckId
                .IsRequired(false);                     // Cho phép ReportCheckId là NULL (nếu Query không tồn tại)

            // Cấu hình Query trong ReportCheck và Appeal là không phân biệt chữ hoa/thường 
            // nếu bạn dùng SQL Server hoặc PostgreSQL
            modelBuilder.Entity<ReportCheck>()
                .Property(rc => rc.Query)
                .IsRequired();

            modelBuilder.Entity<Appeal>()
                .Property(a => a.Query)
                .IsRequired();
        }
    }
}
