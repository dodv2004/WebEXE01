//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using DAL.Models;
//using DAL.Data;

//namespace ProjectEXE01.Controllers
//{
//    public class ReportChecksController : Controller
//    {
//        private readonly ProjectEXE01Context _context;

//        public ReportChecksController(ProjectEXE01Context context)
//        {
//            _context = context;
//        }

//        // GET: ReportChecks
//        public async Task<IActionResult> Index()
//        {
//            return View(await _context.ReportCheck.ToListAsync());
//        }

//        // GET: ReportChecks/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var reportCheck = await _context.ReportCheck
//                .FirstOrDefaultAsync(m => m.Id == id);
//            if (reportCheck == null)
//            {
//                return NotFound();
//            }

//            return View(reportCheck);
//        }

//        // GET: ReportChecks/Create
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // POST: ReportChecks/Create
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("Id,Query,Type,Verdict,ReportCount,CheckedAt,ReporterName,ReporterEmail")] ReportCheck reportCheck)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Add(reportCheck);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(reportCheck);
//        }

//        // GET: ReportChecks/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var reportCheck = await _context.ReportCheck.FindAsync(id);
//            if (reportCheck == null)
//            {
//                return NotFound();
//            }
//            return View(reportCheck);
//        }

//        // POST: ReportChecks/Edit/5
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("Id,Query,Type,Verdict,ReportCount,CheckedAt,ReporterName,ReporterEmail")] ReportCheck reportCheck)
//        {
//            if (id != reportCheck.Id)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    _context.Update(reportCheck);
//                    await _context.SaveChangesAsync();
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!ReportCheckExists(reportCheck.Id))
//                    {
//                        return NotFound();
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                return RedirectToAction(nameof(Index));
//            }
//            return View(reportCheck);
//        }

//        // GET: ReportChecks/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var reportCheck = await _context.ReportCheck
//                .FirstOrDefaultAsync(m => m.Id == id);
//            if (reportCheck == null)
//            {
//                return NotFound();
//            }

//            return View(reportCheck);
//        }

//        // POST: ReportChecks/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var reportCheck = await _context.ReportCheck.FindAsync(id);
//            if (reportCheck != null)
//            {
//                _context.ReportCheck.Remove(reportCheck);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        private bool ReportCheckExists(int id)
//        {
//            return _context.ReportCheck.Any(e => e.Id == id);
//        }
//    }
//}
