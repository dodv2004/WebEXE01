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
        public PaginatedList<User> PagedData { get; set; }

        public async Task OnGetAsync(int p = 1)
        {
            PagedData = await _reportService.GetUsersPagedAsync(p, 5); // 10 user/trang
        }
    }
}