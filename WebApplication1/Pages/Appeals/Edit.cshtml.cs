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

namespace WebAdmin.Pages.Appeals
{
    public class EditModel : PageModel
    {
        private readonly DAL.Data.ProjectEXE01Context _context;

        public EditModel(DAL.Data.ProjectEXE01Context context)
        {
            _context = context;
        }

        [BindProperty]
        public Appeal Appeal { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appeal =  await _context.Appeals.FirstOrDefaultAsync(m => m.Id == id);
            if (appeal == null)
            {
                return NotFound();
            }
            Appeal = appeal;
           ViewData["ReportCheckId"] = new SelectList(_context.ReportChecks, "Id", "Query");
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

            _context.Attach(Appeal).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppealExists(Appeal.Id))
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

        private bool AppealExists(int id)
        {
            return _context.Appeals.Any(e => e.Id == id);
        }
    }
}
