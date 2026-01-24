using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL.Data;
using DAL.Models;

namespace WebAdmin.Pages.Reports
{
    public class DeleteModel : PageModel
    {
        private readonly DAL.Data.ProjectEXE01Context _context;

        public DeleteModel(DAL.Data.ProjectEXE01Context context)
        {
            _context = context;
        }

        [BindProperty]
        public ReportCheck ReportCheck { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reportcheck = await _context.ReportChecks.FirstOrDefaultAsync(m => m.Id == id);

            if (reportcheck == null)
            {
                return NotFound();
            }
            else
            {
                ReportCheck = reportcheck;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reportcheck = await _context.ReportChecks.FindAsync(id);
            if (reportcheck != null)
            {
                ReportCheck = reportcheck;
                _context.ReportChecks.Remove(ReportCheck);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
