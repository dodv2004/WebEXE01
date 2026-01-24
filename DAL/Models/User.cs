namespace DAL.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int SearchCount { get; set; } = 0; // Là int (không phải int?)
        public bool IsVip { get; set; } = false;
        public DateTime LastResetDate { get; set; } = DateTime.Now;
        public string? OTP { get; set; }
        public DateTime? OTPExpiry { get; set; }
        public bool IsActive { get; set; } = false;

        // BỔ SUNG THÊM DÒNG NÀY ĐỂ KHỚP VỚI VIEW
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? VipExpiryDate { get; set; }

    }
}