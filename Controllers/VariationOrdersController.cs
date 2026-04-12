using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectsDashboards.Models;
using ProjectsDashboards.Helpers;

namespace ProjectsDashboards.Controllers
{
    [Authorize(Roles = "Owner,Accountant")]
    public class VariationOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EncryptionHelper _encryption;

        public VariationOrdersController(ApplicationDbContext context, EncryptionHelper encryption)
        {
            _context = context;
            _encryption = encryption;
        }

        // GET: VariationOrders
        public async Task<IActionResult> Index()
        {
            var variationOrders = await _context.VariationOrders
                .Include(v => v.Project)
                .ToListAsync();

            // Decrypt VO Amounts for display
            foreach (var vo in variationOrders)
            {
                if (!string.IsNullOrEmpty(vo.EncryptedVOAmount))
                {
                    vo.VOAmount = _encryption.DecryptPrice(vo.EncryptedVOAmount);
                }
            }

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
                    // Encrypt the VO Amount before saving
                    variationOrder.EncryptedVOAmount = _encryption.EncryptPrice(variationOrder.VOAmount);

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

            // Decrypt VO Amount for display in edit form
            if (!string.IsNullOrEmpty(variationOrder.EncryptedVOAmount))
            {
                variationOrder.VOAmount = _encryption.DecryptPrice(variationOrder.EncryptedVOAmount);
            }

            ViewBag.Projects = _context.Projects.ToList();
            return View(variationOrder);
        }

        // POST: VariationOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,VOAmount,ApprovedDate,Scope")] VariationOrder variationOrder)
        {
            // Check if IDs match
            if (id != variationOrder.Id)
            {
                return NotFound();
            }

            // Remove validation errors for navigation property if any
            if (ModelState.ContainsKey("Project"))
            {
                ModelState.Remove("Project");
            }

            // Check if model is valid
            if (!ModelState.IsValid)
            {
                // If invalid, redisplay the form with errors
                ViewBag.Projects = _context.Projects.ToList();
                return View(variationOrder);
            }

            try
            {
                // Get the existing entity from database
                var existingVO = await _context.VariationOrders.FindAsync(id);
                if (existingVO == null)
                {
                    return NotFound();
                }

                // Update non-encrypted fields
                existingVO.ProjectId = variationOrder.ProjectId;
                existingVO.ApprovedDate = variationOrder.ApprovedDate;
                existingVO.Scope = variationOrder.Scope;

                // Encrypt and update VO Amount
                existingVO.EncryptedVOAmount = _encryption.EncryptPrice(variationOrder.VOAmount);

                // Mark as modified and save
                _context.Update(existingVO);
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
                    ModelState.AddModelError("", "A concurrency error occurred. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unable to save changes. " + ex.Message);
            }

            // If we got this far, something failed, redisplay form
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

            // Decrypt VO Amount for display
            if (!string.IsNullOrEmpty(variationOrder.EncryptedVOAmount))
            {
                variationOrder.VOAmount = _encryption.DecryptPrice(variationOrder.EncryptedVOAmount);
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