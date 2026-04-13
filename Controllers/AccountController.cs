using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectsDashboards.Models;
using System.Security.Claims;
using ProjectsDashboards.Helpers;
using Microsoft.AspNetCore.Http;

namespace ProjectsDashboards.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Get visitor information
            var ipAddress = HttpContextHelper.GetClientIPAddress(HttpContext)?.Substring(0, Math.Min(100, HttpContextHelper.GetClientIPAddress(HttpContext)?.Length ?? 100)) ?? "Unknown";
            var userAgent = HttpContextHelper.GetUserAgent(HttpContext)?.Substring(0, Math.Min(500, HttpContextHelper.GetUserAgent(HttpContext)?.Length ?? 500)) ?? "Unknown";
            var attemptTime = DateTime.Now;

            // Check if this email/name is blocked
            var isBlocked = await _context.BlockedVisitors
                .AnyAsync(b => b.EmailOrName == username &&
                               (b.IsPermanent || b.BlockedUntil > attemptTime));

            if (isBlocked)
            {
                // Log blocked attempt
                var blockedAttempt = new LoginAttempt
                {
                    EmailOrName = username,
                    IPAddress = ipAddress,
                    UserAgent = userAgent,
                    AttemptTime = attemptTime,
                    Status = "Blocked",
                    FailureReason = "User is blocked from accessing the system",
                    FlaggedAs = "Blocked"
                };
                _context.LoginAttempts.Add(blockedAttempt);
                await _context.SaveChangesAsync();

                ViewBag.Error = "Your access has been blocked. Please contact the system administrator.";
                return View();
            }

            // Hash the entered password using SHA256 (same method as when creating user)
            var hashedPassword = HashPassword(password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.FullName == username && u.PasswordHash == hashedPassword);

            if (user != null)
            {
                // SUCCESSFUL LOGIN - Log the attempt
                var successAttempt = new LoginAttempt
                {
                    EmailOrName = username,
                    IPAddress = ipAddress,
                    UserAgent = userAgent,
                    AttemptTime = attemptTime,
                    Status = "Success",
                    AttemptCount = 1
                };
                _context.LoginAttempts.Add(successAttempt);
                await _context.SaveChangesAsync();

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.FullName ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Role, user.Role ?? ""),
            new Claim("UserId", user.ID.ToString())
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Role-based redirect
                switch (user.Role)
                {
                    case "Owner":
                        return RedirectToAction("Dashboard", "Home");

                    case "Staff":
                        return RedirectToAction("Index", "Projects");

                    case "Accountant":
                        // Redirect Accountant to Payment Claims by default
                        return RedirectToAction("Index", "PaymentClaims");

                    default:
                        return RedirectToAction("Login");
                }
            }
            else
            {
                // FAILED LOGIN - Count recent failed attempts
                var recentFailedAttempts = await _context.LoginAttempts
                    .Where(l => l.EmailOrName == username &&
                                l.Status == "Failed" &&
                                l.AttemptTime > attemptTime.AddMinutes(-15))
                    .CountAsync();

                var failedAttemptCount = recentFailedAttempts + 1;

                var failedAttempt = new LoginAttempt
                {
                    EmailOrName = username,
                    IPAddress = ipAddress,
                    UserAgent = userAgent,
                    AttemptTime = attemptTime,
                    Status = "Failed",
                    FailureReason = "Invalid username or password",
                    AttemptCount = failedAttemptCount,
                    FlaggedAs = failedAttemptCount >= 3 ? "Suspicious" : null
                };

                _context.LoginAttempts.Add(failedAttempt);
                await _context.SaveChangesAsync();

                // Auto-red-flag if 5 or more failed attempts in 15 minutes
                if (failedAttemptCount >= 5)
                {
                    failedAttempt.FlaggedAs = "RedFlag";
                    failedAttempt.Notes = $"Multiple failed login attempts ({failedAttemptCount} in last 15 minutes)";
                    await _context.SaveChangesAsync();
                }

                ViewBag.Error = "Invalid username or password";
                return View();
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
