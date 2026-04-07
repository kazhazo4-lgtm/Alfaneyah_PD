using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ProjectsDashboards.Models;

namespace ProjectsDashboards.Controllers
{

    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Projects (Accessible by Owner and Staff)
        [Authorize(Roles = "Owner,Staff,Accountant")]
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Include(p => p.CreatedByUser)
                .Include(p => p.PaymentClaims)
                .Include(p => p.VariationOrders)
                .ToListAsync();
            return View(projects);
        }

        // GET: Projects/Details/5 (Accessible by Owner, Accountant, Staff)
        [Authorize(Roles = "Owner,Accountant,Staff")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.CreatedByUser)
                .Include(p => p.PaymentClaims)
                .Include(p => p.VariationOrders)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create (Accessible by Owner and Staff)
        [Authorize(Roles = "Owner,Staff")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Owner,Staff")]
        public async Task<IActionResult> Create([Bind("ProjectName,ProjectLocation,Scope,ContractStartDate,ContractEndDate,RevisedEndDate,ContractValue")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.CreatedBy = int.Parse(User.FindFirstValue("UserId") ?? "0");
                project.CreatedAt = DateTime.Now;

                _context.Add(project);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Project created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Edit/5 (Accessible by Owner and Staff)
        //[Authorize(Roles = "Owner,Staff")]
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var project = await _context.Projects.FindAsync(id);
        //    if (project == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(project);
        //}

        // GET: Projects/Edit/5 (Accessible by Owner and Staff)
        [Authorize(Roles = "Owner,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.VariationOrders)
                .Include(p => p.PaymentClaims)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Owner,Staff")]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectName,ProjectLocation,Scope,ContractStartDate,ContractEndDate,RevisedEndDate,ContractValue,CreatedBy,CreatedAt")] Project project)
        //{
        //    if (id != project.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(project);
        //            await _context.SaveChangesAsync();
        //            TempData["Success"] = "Project updated successfully!";
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ProjectExists(project.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(project);
        //}

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Owner,Staff")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectName,ProjectLocation,Scope,ContractStartDate,ContractEndDate,RevisedEndDate,ContractValue,CreatedBy,CreatedAt")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Project updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Delete/5 (Accessible by Owner and Staff)
        [Authorize(Roles = "Owner,Staff")] // CHANGED: Added Staff
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.CreatedByUser)
                .Include(p => p.PaymentClaims)
                .Include(p => p.VariationOrders)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            // Show warning if there are related records
            var paymentClaimsCount = project.PaymentClaims?.Count ?? 0;
            var variationOrdersCount = project.VariationOrders?.Count ?? 0;

            if (paymentClaimsCount > 0 || variationOrdersCount > 0)
            {
                ViewBag.WarningMessage = $"This project has {paymentClaimsCount} payment claim(s) and {variationOrdersCount} variation order(s) that will also be deleted.";
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Owner,Staff")] // CHANGED: Added Staff
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects
                .Include(p => p.PaymentClaims)
                .Include(p => p.VariationOrders)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project != null)
            {
                // First delete related Payment Claims if any
                if (project.PaymentClaims != null && project.PaymentClaims.Any())
                {
                    _context.PaymentClaims.RemoveRange(project.PaymentClaims);
                }

                // Then delete related Variation Orders if any
                if (project.VariationOrders != null && project.VariationOrders.Any())
                {
                    _context.VariationOrders.RemoveRange(project.VariationOrders);
                }

                // Finally delete the project
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Project and all related data deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}