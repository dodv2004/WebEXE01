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
    public class DeleteModel : PageModel
    {
        private readonly DAL.Data.ProjectEXE01Context _context;

        public DeleteModel(DAL.Data.ProjectEXE01Context context)
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

            var appeal = await _context.Appeals.FirstOrDefaultAsync(m => m.Id == id);

            if (appeal == null)
            {
                return NotFound();
            }
            else
            {
                Appeal = appeal;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appeal = await _context.Appeals.FindAsync(id);
            if (appeal != null)
            {
                Appeal = appeal;
                _context.Appeals.Remove(Appeal);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
