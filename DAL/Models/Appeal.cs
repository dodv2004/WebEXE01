using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Appeal
    {
        public int Id { get; set; }

        [Required]
        public string Query { get; set; } // Số điện thoại / STK / link bị khiếu nại (đã chuẩn hóa)

        [Required]
        public string AppellantName { get; set; }

        [Required]
        [EmailAddress]
        public string AppellantEmail { get; set; } // Đã chuẩn hóa (lowercase)

        [Required]
        public string Reason { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        // Status: Pending, Accepted, Rejected
        public string Status { get; set; } = "Pending";

        // Khóa ngoại tới ReportCheck (có thể null nếu Query không tồn tại)
        public int? ReportCheckId { get; set; }
        [ForeignKey("ReportCheckId")]
        public ReportCheck? ReportCheck { get; set; }
    }
}
