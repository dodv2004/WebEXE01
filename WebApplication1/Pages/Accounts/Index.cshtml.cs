using BLL.Services;
using DAL.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAdmin.Pages.Accounts
{
    public class IndexModel : PageModel
    {
        private readonly IReportService _reportService;

        // Tiêm Service vào thông qua Constructor
        public IndexModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        // Property ?? View (Index.cshtml) có th? truy c?p d? li?u
        public IEnumerable<User> Users { get; set; } = new List<User>();

        public async Task OnGetAsync()
        {
            // G?i hàm l?y toàn b? ng??i dùng t? t?ng BLL
            var allUsers = await _reportService.GetAllUsersAsync();

            // Gán d? li?u vào Property ?? hi?n th?
            Users = allUsers ?? new List<User>();
        }
    }
}