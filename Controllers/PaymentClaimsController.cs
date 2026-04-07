using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectsDashboards.Models;

namespace ProjectsDashboards.Controllers
{
    [Authorize(Roles = "Owner,Accountant")]
    public class PaymentClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentClaimsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PaymentClaims
        public async Task<IActionResult> Index()
        {
            var paymentClaims = await _context.Set<PaymentClaim>()
                .Include(p => p.Project)
                .ToListAsync();
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
                    _context.Set<PaymentClaim>().Update(paymentClaim);
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