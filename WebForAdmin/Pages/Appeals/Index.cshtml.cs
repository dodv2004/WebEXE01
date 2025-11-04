using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL.Data;
using DAL.Models;

namespace WebForAdmin.Pages.Appeals
{
    public class IndexModel : PageModel
    {
        private readonly DAL.Data.ProjectEXE01Context _context;

        public IndexModel(DAL.Data.ProjectEXE01Context context)
        {
            _context = context;
        }

        public IList<Appeal> Appeal { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Appeal = await _context.Appeals
                .Include(a => a.ReportCheck).ToListAsync();
        }
    }
}
