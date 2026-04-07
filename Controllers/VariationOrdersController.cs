using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectsDashboards.Models;

namespace ProjectsDashboards.Controllers
{
    [Authorize(Roles = "Owner,Accountant")]
    public class VariationOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VariationOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VariationOrders
        public async Task<IActionResult> Index()
        {
            var variationOrders = await _context.VariationOrders
                .Include(v => v.Project)
                .ToListAsync();
            return View(variationOrders);
        }

        // GET: VariationOrders/Create
        public IActionResult Create()
        {
            ViewBag.Projects = _context.Projects.ToList();
            return View();
        }

        // POST: VariationOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,VOAmount,ApprovedDate,Scope")] VariationOrder variationOrder)
        {
            // Remove validation errors for navigation property if any
            if (ModelState.ContainsKey("Project"))
            {
                ModelState.Remove("Project");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(variationOrder);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Variation order added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. " + ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Projects = _context.Projects.ToList();
            return View(variationOrder);
        }

        // GET: VariationOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var variationOrder = await _context.VariationOrders.FindAsync(id);
            if (variationOrder == null)
            {
                return NotFound();
            }
            ViewBag.Projects = _context.Projects.ToList();
            return View(variationOrder);
        }

        // POST: VariationOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,VOAmount,ApprovedDate,Scope")] VariationOrder variationOrder)
        {
            if (id != variationOrder.Id)
            {
                return NotFound();
            }

            // Remove validation errors for navigation property if any
            if (ModelState.ContainsKey("Project"))
            {
                ModelState.Remove("Project");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(variationOrder);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Variation order updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VariationOrderExists(variationOrder.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. " + ex.Message);
                }
            }

            ViewBag.Projects = _context.Projects.ToList();
            return View(variationOrder);
        }

        // GET: VariationOrders/Delete/5
        [Authorize(Roles = "Owner,Accountant")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var variationOrder = await _context.VariationOrders
                .Include(v => v.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (variationOrder == null)
            {
                return NotFound();
            }

            return View(variationOrder);
        }

        // POST: VariationOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Owner,Accountant")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var variationOrder = await _context.VariationOrders.FindAsync(id);
            if (variationOrder != null)
            {
                _context.VariationOrders.Remove(variationOrder);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Variation order deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VariationOrderExists(int id)
        {
            return _context.VariationOrders.Any(e => e.Id == id);
        }
    }
}