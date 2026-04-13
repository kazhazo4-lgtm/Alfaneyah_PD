using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectsDashboards.Models;
using ProjectsDashboards.Helpers;
using System.Security.Claims;

namespace ProjectsDashboards.Controllers
{
    [Authorize(Policy = "OwnerOnly")]
    public class VisitorManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VisitorManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VisitorManagement
        public async Task<IActionResult> Index()
        {
            var viewModel = new VisitorManagementViewModel();

            // Get all flagged visitors (Red Flag, Suspicious, Blocked)
            viewModel.FlaggedVisitors = await _context.LoginAttempts
                .Where(l => l.FlaggedAs == "RedFlag" || l.FlaggedAs == "Suspicious" || l.FlaggedAs == "Blocked")
                .OrderByDescending(l => l.AttemptTime)
                .GroupBy(l => l.EmailOrName)
                .Select(g => g.First())
                .ToListAsync();

            // Get all blocked visitors
            viewModel.BlockedVisitors = await _context.BlockedVisitors
                .Include(b => b.BlockedByUser)
                .OrderByDescending(b => b.BlockedAt)
                .ToListAsync();

            // Get recent login attempts (last 7 days)
            var sevenDaysAgo = DateTime.Now.AddDays(-7);
            viewModel.RecentAttempts = await _context.LoginAttempts
                .Where(l => l.AttemptTime > sevenDaysAgo)
                .OrderByDescending(l => l.AttemptTime)
                .Take(100)
                .ToListAsync();

            // Get summary statistics
            viewModel.TotalFailedAttempts = await _context.LoginAttempts
                .CountAsync(l => l.Status == "Failed" && l.AttemptTime > sevenDaysAgo);

            viewModel.TotalRedFlags = viewModel.FlaggedVisitors.Count();
            viewModel.TotalBlocked = viewModel.BlockedVisitors.Count();

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAndCreateUser(int attemptId, string role, string fullName, string email)
        {
            var attempt = await _context.LoginAttempts.FindAsync(attemptId);
            if (attempt == null)
            {
                return NotFound();
            }

            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.FullName == fullName || u.Email == email);

            if (existingUser != null)
            {
                TempData["Error"] = "A user with this name or email already exists.";
                return RedirectToAction(nameof(Index));
            }

            // Generate random password (user will change on first login)
            var tempPassword = GenerateRandomPassword();
            var hashedPassword = HashPassword(tempPassword);

            var newUser = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = hashedPassword,
                Role = role
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Update the login attempt
            attempt.Status = "Approved";
            attempt.ApprovedUserId = newUser.ID;
            attempt.FlaggedAs = null;
            attempt.Notes = $"Approved by Owner. User created with role: {role}";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User '{fullName}' created successfully with role '{role}'. Temporary password: {tempPassword} (user should change on first login)";
            return RedirectToAction(nameof(Index));
        }

        // POST: VisitorManagement/BlockVisitor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockVisitor(string emailOrName, string reason, bool isPermanent, int? daysToBlock)
        {
            var currentUserId = int.Parse(User.FindFirstValue("UserId") ?? "0");

            var blockedVisitor = new BlockedVisitor
            {
                EmailOrName = emailOrName,
                BlockedAt = DateTime.Now,
                BlockedByUserId = currentUserId,
                Reason = reason,
                IsPermanent = isPermanent,
                BlockedUntil = isPermanent ? null : DateTime.Now.AddDays(daysToBlock ?? 30)
            };

            _context.BlockedVisitors.Add(blockedVisitor);

            // Update all login attempts for this email/name
            var attempts = await _context.LoginAttempts
                .Where(l => l.EmailOrName == emailOrName)
                .ToListAsync();

            foreach (var attempt in attempts)
            {
                attempt.FlaggedAs = "Blocked";
                attempt.IsBlocked = true;
                attempt.BlockedUntil = blockedVisitor.BlockedUntil;
                attempt.Notes = $"Blocked by Owner. Reason: {reason}";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Visitor '{emailOrName}' has been blocked.";
            return RedirectToAction(nameof(Index));
        }

        // POST: VisitorManagement/UnblockVisitor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockVisitor(int blockId)
        {
            var blockedVisitor = await _context.BlockedVisitors.FindAsync(blockId);
            if (blockedVisitor != null)
            {
                _context.BlockedVisitors.Remove(blockedVisitor);

                // Remove block flag from login attempts
                var attempts = await _context.LoginAttempts
                    .Where(l => l.EmailOrName == blockedVisitor.EmailOrName)
                    .ToListAsync();

                foreach (var attempt in attempts)
                {
                    attempt.FlaggedAs = null;
                    attempt.IsBlocked = false;
                    attempt.BlockedUntil = null;
                    attempt.Notes = "Unblocked by Owner";
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Visitor '{blockedVisitor.EmailOrName}' has been unblocked.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: VisitorManagement/RemoveRedFlag
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRedFlag(int attemptId)
        {
            var attempt = await _context.LoginAttempts.FindAsync(attemptId);
            if (attempt != null)
            {
                attempt.FlaggedAs = null;
                attempt.Notes = "Red flag removed by Owner (recognized as legitimate)";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Red flag removed.";
            }

            return RedirectToAction(nameof(Index));
        }

        private string GenerateRandomPassword()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }

    public class VisitorManagementViewModel
    {
        public List<LoginAttempt>? FlaggedVisitors { get; set; }
        public List<BlockedVisitor>? BlockedVisitors { get; set; }
        public List<LoginAttempt>? RecentAttempts { get; set; }
        public int TotalFailedAttempts { get; set; }
        public int TotalRedFlags { get; set; }
        public int TotalBlocked { get; set; }
    }
}