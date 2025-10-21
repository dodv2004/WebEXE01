using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Result
    {
        public string Query { get; set; }
        public bool Found { get; set; }

        // Nếu tìm thấy
        public string Type { get; set; }
        public string Verdict { get; set; }
        public int ReportCount { get; set; }
        public DateTime CheckedAt { get; set; }

        // Nếu có thông tin người báo cáo
        public List<string> Reporters { get; set; } = new();
    }
}
