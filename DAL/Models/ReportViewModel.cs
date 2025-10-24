using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ReportViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại, STK hoặc link")]
        public string Query { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại báo cáo")]
        public string Type { get; set; }

        public string Note { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên của bạn")]
        public string ReporterName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string ReporterEmail { get; set; }
    }
}
