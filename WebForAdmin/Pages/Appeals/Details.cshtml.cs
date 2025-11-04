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
    public class DetailsModel : PageModel
    {
        private readonly DAL.Data.ProjectEXE01Context _context;

        public DetailsModel(DAL.Data.ProjectEXE01Context context)
        {
            _context = context;
        }

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
    }
}
