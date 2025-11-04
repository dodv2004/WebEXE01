using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ReportCheck
    {
        public int Id { get; set; }

        [Required]
        public string Query { get; set; } // Số điện thoại / STK / link
        public string Type { get; set; }  // phone, bank, link
        public string Verdict { get; set; } // "Đã bị báo cáo lừa đảo", "An toàn", ...
        public int ReportCount { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.Now;

        // Quan hệ N-N với Reporter
        public ICollection<ReporterReportCheck> ReporterReportChecks { get; set; }
        // Quan hệ 1-N với Appeal
        public ICollection<Appeal> Appeals { get; set; } = new List<Appeal>();
    }
}

