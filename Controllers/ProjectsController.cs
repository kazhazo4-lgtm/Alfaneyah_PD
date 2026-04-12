using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ProjectsDashboards.Models;
using ProjectsDashboards.Helpers;

namespace ProjectsDashboards.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EncryptionHelper _encryption;  // ← ADD THIS

        public ProjectsController(ApplicationDbContext context, EncryptionHelper encryption)  // ← UPDATE CONSTRUCTOR
        {
            _context = context;
            _encryption = encryption;
        }

        [Authorize(Roles = "Owner,Staff,Accountant")]
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Include(p => p.CreatedByUser)
                .Include(p => p.PaymentClaims)
                .Include(p => p.VariationOrders)
                .ToListAsync();

            // ========== DECRYPT ALL FINANCIAL DATA ==========
            foreach (var project in projects)
            {
                // Decrypt Contract Value
                if (!string.IsNullOrEmpty(project.EncryptedContractValue))
                {
                    project.ContractValue = _encryption.DecryptPriceNullable(project.EncryptedContractValue);
                }

                // Decrypt Payment Claims
                if (project.PaymentClaims != null)
                {
                    foreach (var claim in project.PaymentClaims)
                    {
                        if (!string.IsNullOrEmpty(claim.EncryptedClaimAmount))
                        {
                            claim.ClaimAmount = _encryption.DecryptPrice(claim.EncryptedClaimAmount);
                        }
                    }
                }

                // Decrypt Variation Orders
                if (project.VariationOrders != null)
                {
                    foreach (var vo in project.VariationOrders)
                    {
                        if (!string.IsNullOrEmpty(vo.EncryptedVOAmount))
                        {
                            vo.VOAmount = _encryption.DecryptPrice(vo.EncryptedVOAmount);
                        }
                    }
                }
            }

            return View(projects);
        }

        [Authorize(Roles = "Owner,Accountant,Staff")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.CreatedByUser)
                .Include(p => p.PaymentClaims)
                .Include(p => p.VariationOrders)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            // Decrypt Contract Value
            if (!string.IsNullOrEmpty(project.EncryptedContractValue))
            {
                project.ContractValue = _encryption.DecryptPriceNullable(project.EncryptedContractValue);
            }

            // Decrypt Payment Claims
            if (project.PaymentClaims != null)
            {
                foreach (var claim in project.PaymentClaims)
                {
                    if (!string.IsNullOrEmpty(claim.EncryptedClaimAmount))
                    {
                        claim.ClaimAmount = _encryption.DecryptPrice(claim.EncryptedClaimAmount);
                    }
                }
            }

            // Decrypt Variation Orders
            if (project.VariationOrders != null)
            {
                foreach (var vo in project.VariationOrders)
                {
                    if (!string.IsNullOrEmpty(vo.EncryptedVOAmount))
                    {
                        vo.VOAmount = _encryption.DecryptPrice(vo.EncryptedVOAmount);
                    }
                }
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
                // Encrypt the Contract Value before saving
                if (project.ContractValue.HasValue)
                {
                    project.EncryptedContractValue = _encryption.EncryptPriceNullable(project.ContractValue);
                }

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

            // Decrypt Contract Value for display in edit form
            if (!string.IsNullOrEmpty(project.EncryptedContractValue))
            {
                project.ContractValue = _encryption.DecryptPriceNullable(project.EncryptedContractValue);
            }

            return View(project);
        }

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
                    var existingProject = await _context.Projects.FindAsync(id);
                    if (existingProject == null)
                    {
                        return NotFound();
                    }

                    // Update non-encrypted fields
                    existingProject.ProjectName = project.ProjectName;
                    existingProject.ProjectLocation = project.ProjectLocation;
                    existingProject.Scope = project.Scope;
                    existingProject.ContractStartDate = project.ContractStartDate;
                    existingProject.ContractEndDate = project.ContractEndDate;
                    existingProject.RevisedEndDate = project.RevisedEndDate;
                    existingProject.CreatedBy = project.CreatedBy;
                    existingProject.CreatedAt = project.CreatedAt;

                    // Encrypt and update Contract Value if changed
                    if (project.ContractValue.HasValue)
                    {
                        existingProject.EncryptedContractValue = _encryption.EncryptPriceNullable(project.ContractValue);
                    }

                    _context.Update(existingProject);
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
        [Authorize(Roles = "Owner,Staff")]
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

            // Decrypt for display
            if (!string.IsNullOrEmpty(project.EncryptedContractValue))
            {
                project.ContractValue = _encryption.DecryptPriceNullable(project.EncryptedContractValue);
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
        [Authorize(Roles = "Owner,Staff")]
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