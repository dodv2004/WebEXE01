using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL.Data;
using DAL.Models;

namespace WebAdmin.Pages.Reports
{
    public class CreateModel : PageModel
    {
        private readonly DAL.Data.ProjectEXE01Context _context;

        public CreateModel(DAL.Data.ProjectEXE01Context context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public ReportCheck ReportCheck { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.ReportChecks.Add(ReportCheck);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
