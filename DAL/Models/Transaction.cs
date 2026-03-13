using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; } // ID người mua
        public string Email { get; set; } = string.Empty;
        public decimal Amount { get; set; } = 89000; // Giá cố định
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string PaymentMethod { get; set; } = "MoMo";
        public string TransactionNote { get; set; } = string.Empty; // Nội dung chuyển khoản
        public int Status { get; set; }

        // Navigation property
        public virtual User? User { get; set; }
    }
}
