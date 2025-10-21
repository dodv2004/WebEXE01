using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ReporterReportCheck
    {
        public int ReporterId { get; set; }
        public Reporter Reporter { get; set; }

        public int ReportCheckId { get; set; }
        public ReportCheck ReportCheck { get; set; }

        public DateTime ReportedAt { get; set; } = DateTime.Now;
        public string Note { get; set; } // Lý do báo cáo (tùy chọn)
    }

}
