using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ReporterRankingDto
    {
        public string Name { get; set; } = string.Empty;
        public string MaskedEmail { get; set; } = string.Empty; // Để ẩn bớt email
        public int TotalReports { get; set; }
    }
}
