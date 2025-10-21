using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Reporter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        // Quan hệ N-N với ReportCheck
        public ICollection<ReporterReportCheck> ReporterReportChecks { get; set; }
    }

}
