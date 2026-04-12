using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectsDashboards.Models;
using ProjectsDashboards.Helpers;  // ← ADD THIS

namespace ProjectsDashboards.Controllers
{
    [Authorize(Roles = "Owner,Accountant")]
    public class PaymentClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EncryptionHelper _encryption;  // ← ADD THIS

        public PaymentClaimsController(ApplicationDbContext context, EncryptionHelper encryption)  // ← UPDATE CONSTRUCTOR
        {
            _context = context;
            _encryption = encryption;  // ← ADD THIS
        }

        // GET: PaymentClaims
        public async Task<IActionResult> Index()
        {
            var paymentClaims = await _context.Set<PaymentClaim>()
                .Include(p => p.Project)
                .ToListAsync();

            // Decrypt Claim Amounts for display
            foreach (var claim in paymentClaims)
            {
                if (!string.IsNullOrEmpty(claim.EncryptedClaimAmount))
                {
                    claim.ClaimAmount = _encryption.DecryptPrice(claim.EncryptedClaimAmount);
                }
            }

            return View(paymentClaims);
        }

        // GET: PaymentClaims/Create
        public IActionResult Create()
        {
            ViewBag.Projects = _context.Projects.ToList();
            return View();
        }

        // POST: PaymentClaims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,ClaimAmount,ClaimDate,Description,MonthPayment,ApprovedDate,VATDate")] PaymentClaim paymentClaim)
        {
            if (ModelState.IsValid)
            {
                // Encrypt the Claim Amount before saving
                paymentClaim.EncryptedClaimAmount = _encryption.EncryptPrice(paymentClaim.ClaimAmount);

                _context.Set<PaymentClaim>().Add(paymentClaim);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Payment claim added successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Projects = _context.Projects.ToList();
            return View(paymentClaim);
        }

        // GET: PaymentClaims/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentClaim = await _context.Set<PaymentClaim>()
                .Include(p => p.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (paymentClaim == null)
            {
                return NotFound();
            }

            // Decrypt Claim Amount for display in edit form
            if (!string.IsNullOrEmpty(paymentClaim.EncryptedClaimAmount))
            {
                paymentClaim.ClaimAmount = _encryption.DecryptPrice(paymentClaim.EncryptedClaimAmount);
            }

            ViewBag.Projects = _context.Projects.ToList();
            return View(paymentClaim);
        }

        // POST: PaymentClaims/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,ClaimAmount,ClaimDate,Description,MonthPayment,ApprovedDate,VATDate")] PaymentClaim paymentClaim)
        {
            if (id != paymentClaim.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingClaim = await _context.Set<PaymentClaim>().FindAsync(id);
                    if (existingClaim == null)
                    {
                        return NotFound();
                    }

                    // Update non-encrypted fields
                    existingClaim.ProjectId = paymentClaim.ProjectId;
                    existingClaim.ClaimDate = paymentClaim.ClaimDate;
                    existingClaim.Description = paymentClaim.Description;
                    existingClaim.MonthPayment = paymentClaim.MonthPayment;
                    existingClaim.ApprovedDate = paymentClaim.ApprovedDate;
                    existingClaim.VATDate = paymentClaim.VATDate;

                    // Encrypt and update Claim Amount
                    existingClaim.EncryptedClaimAmount = _encryption.EncryptPrice(paymentClaim.ClaimAmount);

                    _context.Set<PaymentClaim>().Update(existingClaim);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Payment claim updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentClaimExists(paymentClaim.Id))
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
            ViewBag.Projects = _context.Projects.ToList();
            return View(paymentClaim);
        }

        // GET: PaymentClaims/Delete/5
        [Authorize(Roles = "Owner,Accountant")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentClaim = await _context.Set<PaymentClaim>()
                .Include(p => p.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (paymentClaim == null)
            {
                return NotFound();
            }

            // Decrypt Claim Amount for display
            if (!string.IsNullOrEmpty(paymentClaim.EncryptedClaimAmount))
            {
                paymentClaim.ClaimAmount = _encryption.DecryptPrice(paymentClaim.EncryptedClaimAmount);
            }

            return View(paymentClaim);
        }

        // POST: PaymentClaims/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Owner,Accountant")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paymentClaim = await _context.Set<PaymentClaim>().FindAsync(id);
            if (paymentClaim != null)
            {
                _context.Set<PaymentClaim>().Remove(paymentClaim);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Payment claim deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PaymentClaimExists(int id)
        {
            return _context.Set<PaymentClaim>().Any(e => e.Id == id);
        }
    }
}