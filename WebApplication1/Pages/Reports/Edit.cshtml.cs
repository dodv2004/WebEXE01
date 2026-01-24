using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DAL.Data;
using DAL.Models;

namespace WebAdmin.Pages.Reports
{
    public class EditModel : PageModel
    {
        private readonly DAL.Data.ProjectEXE01Context _context;

        public EditModel(DAL.Data.ProjectEXE01Context context)
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

            var reportcheck =  await _context.ReportChecks.FirstOrDefaultAsync(m => m.Id == id);
            if (reportcheck == null)
            {
                return NotFound();
            }
            ReportCheck = reportcheck;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(ReportCheck).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportCheckExists(ReportCheck.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ReportCheckExists(int id)
        {
            return _context.ReportChecks.Any(e => e.Id == id);
        }
    }
}
