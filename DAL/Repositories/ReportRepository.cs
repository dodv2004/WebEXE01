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
            // Mặc định Status là 0 (Pending)
            _context.Transactions.Add(transaction);
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
        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions.FindAsync(id);
        }
        // Chỉ lấy tổng doanh thu từ các giao dịch đã được duyệt (Status = 1)
        public async Task<decimal> GetTotalConfirmedRevenueAsync()
        {
            return await _context.Transactions
                .Where(t => t.Status == 1)
                .SumAsync(t => t.Amount);
        }

        // Lấy doanh thu tháng hiện tại đã duyệt
        public async Task<decimal> GetMonthlyConfirmedRevenueAsync()
        {
            var now = DateTime.Now;
            return await _context.Transactions
                .Where(t => t.Status == 1 && t.PaymentDate.Month == now.Month && t.PaymentDate.Year == now.Year)
                .SumAsync(t => t.Amount);
        }
        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await SaveAsync(); // Lưu thay đổi ngay để cập nhật Status
        }
        public async Task<PaginatedList<Transaction>> GetTransactionsPagedAsync(int pageIndex, int pageSize)
        {
            var count = await _context.Transactions.CountAsync();
            var items = await _context.Transactions
                .OrderByDescending(t => t.PaymentDate)
                .Skip((pageIndex - 1) * pageSize) // Bỏ qua các trang trước
                .Take(pageSize)                   // Chỉ lấy đủ số lượng trang hiện tại
                .ToListAsync();

            return new PaginatedList<Transaction>(items, count, pageIndex, pageSize);
        }
        // Phân trang cho Người dùng
        public async Task<PaginatedList<User>> GetUsersPagedAsync(int pageIndex, int pageSize)
        {
            var count = await _context.Users.CountAsync();
            var items = await _context.Users
                .OrderByDescending(u => u.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<User>(items, count, pageIndex, pageSize);
        }

        // Phân trang cho Báo cáo
        public async Task<PaginatedList<ReportCheck>> GetReportsPagedAsync(int pageIndex, int pageSize)
        {
            var count = await _context.ReportChecks.CountAsync();
            var items = await _context.ReportChecks
                .OrderByDescending(r => r.CheckedAt)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<ReportCheck>(items, count, pageIndex, pageSize);
        }

        // Phân trang cho Khiếu nại
        public async Task<PaginatedList<Appeal>> GetAppealsPagedAsync(int pageIndex, int pageSize)
        {
            var count = await _context.Appeals.CountAsync();
            var items = await _context.Appeals
                .Include(a => a.ReportCheck)
                .OrderByDescending(a => a.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<Appeal>(items, count, pageIndex, pageSize);
        }
        public async Task<bool> HasPendingTransactionAsync(string email)
        {
            // Status = 0 là đang chờ duyệt
            return await _context.Transactions
                .AnyAsync(t => t.Email == email.ToLower().Trim() && t.Status == 0);
        }
    }
}