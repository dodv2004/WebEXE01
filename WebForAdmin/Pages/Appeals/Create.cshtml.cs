using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL.Data;
using DAL.Models;

namespace WebForAdmin.Pages.Appeals
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
        ViewData["ReportCheckId"] = new SelectList(_context.ReportChecks, "Id", "Query");
            return Page();
        }

        [BindProperty]
        public Appeal Appeal { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Appeals.Add(Appeal);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
