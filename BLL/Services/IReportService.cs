using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IReportService
    {
        // --- CHỨC NĂNG TRA CỨU & BÁO CÁO ---
        Task<(bool Success, string Message, Result? ResultData)> SearchAsync(string query);
        Task<IEnumerable<ReportCheck>> GetAllAsync();
        Task<(bool Success, string Message)> AddReportAsync(string query, string type, string reporterName, string reporterEmail, string note);
        Task<(bool Success, string Message)> SubmitAppealAsync(string query, string name, string email, string reason);

        // --- CHỨC NĂNG ADMIN - THỐNG KÊ ---
        Task<int> GetReportCountAsync();
        Task<int> GetPendingReportCountAsync();
        Task<int> GetPendingAppealCountAsync();

        // --- CHỨC NĂNG ADMIN - XỬ LÝ ---
        Task<IEnumerable<ReportCheck>> GetAllReportsAsync();
        Task<IEnumerable<Appeal>> GetPendingAppealsAsync();
        Task<(bool Success, string Message)> UpdateReportVerdictAsync(int reportCheckId, string newVerdict);
        Task<(bool Success, string Message)> ProcessAppealAsync(int appealId, bool acceptAppeal);

        // --- CHỨC NĂNG NGƯỜI DÙNG & BẢO MẬT (Cập nhật mới) ---

        // 1. Đăng ký tài khoản (Gửi OTP lần 1)
        Task<(bool Success, string Message)> RegisterAsync(string email, string password);

        // 2. Xác thực OTP để kích hoạt tài khoản hoặc xác nhận quên mật khẩu
        Task<(bool Success, string Message)> VerifyOTPAsync(string email, string otp);

        // 3. Đăng nhập (Chỉ cho phép nếu tài khoản đã active qua OTP)
        Task<(bool Success, string Message)> LoginAsync(string email, string password);

        // 4. Quên mật khẩu (Gửi OTP lần 2)
        Task<(bool Success, string Message)> ForgotPasswordAsync(string email);

        // 5. Đặt lại mật khẩu mới (Sau khi đã có mã OTP hợp lệ)
        Task<(bool Success, string Message)> ResetPasswordWithOTPAsync(string email, string otp, string newPassword);

        // 6. Kiểm tra giới hạn tra cứu (15 lượt/tháng)
        Task<(bool CanSearch, string Message)> CheckAndIncrementSearchLimitAsync(string email);

        // Thêm vào IReportService.cs
        Task<(bool Success, string Message, User? UserData)> GetUserProfileAsync(string email);

        // Kiểm tra xem User có được phép tra cứu tiếp hay không
        Task<(bool CanSearch, string Message)> CheckSearchPermissionAsync(string email);

        // Tăng số lượt tra cứu (chỉ áp dụng cho tài khoản thường)
        Task IncrementSearchCountAsync(string email);
   
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task<IEnumerable<ReporterRankingDto>> GetTopReportersAsync(int count);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> CreatePendingTransactionAsync(string email, decimal amount);
        Task<(bool Success, string Message)> ApproveTransactionAsync(int transactionId);
        Task<PaginatedList<Transaction>> GetTransactionsPagedAsync(int pageIndex, int pageSize);
        Task<PaginatedList<User>> GetUsersPagedAsync(int pageIndex, int pageSize);
        Task<PaginatedList<ReportCheck>> GetReportsPagedAsync(int pageIndex, int pageSize);
        Task<PaginatedList<Appeal>> GetAppealsPagedAsync(int pageIndex, int pageSize);
        Task<bool> CheckHasPendingTransactionAsync(string email);
        Task<(bool Success, string Message)> RejectTransactionAsync(int transactionId);
    }
}