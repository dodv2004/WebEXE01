using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class AppealViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin bị báo cáo sai.")]
        public string Query { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên của bạn.")]
        public string AppellantName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email liên hệ.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string AppellantEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nêu rõ lý do khiếu nại.")]
        public string Reason { get; set; }
    }
}
