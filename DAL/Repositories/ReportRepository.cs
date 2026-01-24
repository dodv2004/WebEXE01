using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ProjectEXE01Context _context;

        public ReportRepository(ProjectEXE01Context context)
        {
            _context = context;
        }

        public async Task<ReportCheck?> GetByQueryAsync(string query)
        {
            // Tối ưu hóa: Lấy tất cả thông tin quan hệ cần thiết cho việc hiển thị kết quả
            return await _context.ReportChecks
                .Include(rc => rc.ReporterReportChecks)
                    .ThenInclude(rr => rr.Reporter)
                .FirstOrDefaultAsync(rc => rc.Query == query.ToLower().Trim()); // So sánh không phân biệt chữ hoa/thường
        }

        public async Task<IEnumerable<ReportCheck>> GetAllAsync()
        {
            return await _context.ReportChecks
                .Include(rc => rc.ReporterReportChecks)
                .ThenInclude(rr => rr.Reporter)
                .OrderByDescending(rc => rc.CheckedAt)
                .ToListAsync();
        }

        public async Task<bool> AddReportAsync(string query, string type, Reporter reporter, string note)
        {
            var cleanQuery = query.ToLower().Trim();

            // 1. Tìm hoặc tạo ReportCheck (Phần này ổn)
            var existingReportCheck = await _context.ReportChecks
                .FirstOrDefaultAsync(rc => rc.Query == cleanQuery);

            if (existingReportCheck == null)
            {
                existingReportCheck = new ReportCheck
                {
                    Query = cleanQuery,
                    Type = type,
                    Verdict = "Chờ xác minh (Chưa được Admin duyệt)",
                    ReportCount = 0
                };
                _context.ReportChecks.Add(existingReportCheck);
                await SaveAsync(); // Bắt buộc ReportCheck có Id trước
            }

            // 2. Tìm hoặc tạo Reporter
            var existingReporter = await _context.Reporters
                .FirstOrDefaultAsync(r => r.Email == reporter.Email.ToLower().Trim());

            // Xử lý Reporter mới
            if (existingReporter == null)
            {
                reporter.Email = reporter.Email.ToLower().Trim();
                _context.Reporters.Add(reporter);
                await SaveAsync(); // *** LƯU REPORTER MỚI để nó có ID ***
                existingReporter = reporter; // Gán lại để sử dụng đối tượng có ID
            }

            // 3. Kiểm tra quan hệ đã tồn tại (Reporter đã báo cáo mục này chưa)
            var existingRelation = await _context.ReporterReportChecks
                .FirstOrDefaultAsync(rr =>
                    rr.ReporterId == existingReporter.Id && // ID này bây giờ đã chắc chắn có giá trị
                    rr.ReportCheckId == existingReportCheck.Id);

            if (existingRelation != null)
            {
                return false; // Reporter đã báo cáo mục này trước đó
            }

            // 4. Tạo quan hệ mới
            existingReportCheck.ReportCount++;
            var relation = new ReporterReportCheck
            {
                ReporterId = existingReporter.Id,
                ReportCheckId = existingReportCheck.Id,
                ReportedAt = System.DateTime.Now,
                Note = note
            };
            _context.ReporterReportChecks.Add(relation);

            await SaveAsync(); // Lần Save cuối cùng
            return true;
        }

        // --- CHỨC NĂNG KHIẾU NẠI ---

        public async Task<Appeal?> GetPendingAppealByQueryAndEmailAsync(string query, string email)
        {
            var cleanQuery = query.ToLower().Trim();
            var cleanEmail = email.ToLower().Trim();

            return await _context.Appeals
                .FirstOrDefaultAsync(a =>
                    a.Query == cleanQuery &&
                    a.AppellantEmail == cleanEmail &&
                    a.Status == "Pending");
        }

        public async Task AddAppealAsync(Appeal appeal)
        {
            var cleanQuery = appeal.Query.ToLower().Trim();

            var existingReportCheck = await _context.ReportChecks
                .FirstOrDefaultAsync(rc => rc.Query == cleanQuery);

            if (existingReportCheck != null)
            {
                appeal.ReportCheckId = existingReportCheck.Id;
            }

            appeal.Query = cleanQuery;
            appeal.AppellantEmail = appeal.AppellantEmail.ToLower().Trim();
            appeal.Status = "Pending";

            _context.Appeals.Add(appeal);
            await SaveAsync();
        }

        // --- CHỨC NĂNG ADMIN ---

        public async Task<IEnumerable<Appeal>> GetAllAppealsAsync()
        {
            return await _context.Appeals
               .Include(a => a.ReportCheck) // Quan trọng cho việc hiển thị trong Admin
               .ToListAsync();
        }
        public async Task UpdateReportCheckVerdict(int reportCheckId, string newVerdict)
        {
            var reportCheck = await _context.ReportChecks.FindAsync(reportCheckId);
            if (reportCheck != null)
            {
                reportCheck.Verdict = newVerdict;
                // Reset ReportCount nếu chuyển về An toàn
                if (newVerdict.Contains("An toàn"))
                {
                    reportCheck.ReportCount = 0;
                }
                _context.ReportChecks.Update(reportCheck);
                await SaveAsync();
            }
        }

        public async Task UpdateAppealStatus(int appealId, string newStatus)
        {
            var appeal = await _context.Appeals.FindAsync(appealId);
            if (appeal != null)
            {
                appeal.Status = newStatus;
                _context.Appeals.Update(appeal);
                await SaveAsync();
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }


        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());
        }

        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await SaveAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await SaveAsync();
        }

        // Triển khai hàm thêm giao dịch mới
        public async Task AddTransactionAsync(Transaction transaction)
        {
            // Thêm bản ghi vào DbSet Transactions
            _context.Transactions.Add(transaction);

            // Gọi hàm SaveAsync() có sẵn của bạn để hoàn tất lưu xuống SQL Server
            await SaveAsync();
        }

        // Triển khai hàm lấy toàn bộ lịch sử để quản lý doanh thu
        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions
                .OrderByDescending(t => t.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReporterRankingDto>> GetTopReportersAsync(int count)
        {
            return await _context.Reporters
                .Select(r => new ReporterRankingDto
                {
                    Name = r.Name,
                    // Ví dụ: abc...z@gmail.com
                    MaskedEmail = r.Email.Substring(0, 1) + "..." + r.Email.Substring(r.Email.IndexOf("@") - 1),
                    TotalReports = r.ReporterReportChecks.Count()
                })
                .OrderByDescending(r => r.TotalReports)
                .Take(count)
                .ToListAsync();
        }

        // Triển khai hàm lấy danh sách người dùng
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            // Lấy toàn bộ danh sách, sắp xếp theo ID giảm dần
            return await _context.Users
                .OrderByDescending(u => u.Id)
                .ToListAsync();
        }

        
    }
}