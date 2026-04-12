using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectsDashboards.Models;
using System.Security.Cryptography;
using System.Text;

namespace ProjectsDashboards.Controllers
{
    // Only Owner can access User Management
    [Authorize(Policy = "OwnerOnly")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.ID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,Role")] User user, string PlainPassword)
        {
            // Check if password was provided
            if (string.IsNullOrEmpty(PlainPassword))
            {
                ModelState.AddModelError("", "Password is required.");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == user.Email || u.FullName == user.FullName);

                if (existingUser != null)
                {
                    ModelState.AddModelError("", "A user with this name or email already exists.");
                    return View(user);
                }

                // Hash the password before saving
                user.PasswordHash = HashPassword(PlainPassword);

                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"User '{user.FullName}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,FullName,Email,Role")] User user, string NewPassword)
        {
            if (id != user.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Update basic info
                    existingUser.FullName = user.FullName;
                    existingUser.Email = user.Email;
                    existingUser.Role = user.Role;

                    // Update password only if provided
                    if (!string.IsNullOrEmpty(NewPassword))
                    {
                        existingUser.PasswordHash = HashPassword(NewPassword);
                    }

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"User '{existingUser.FullName}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.ID))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.ID == id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting the current logged-in user
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (user.ID == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);

            // Prevent deleting the current logged-in user
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (user != null && user.ID != currentUserId)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"User '{user.FullName}' deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Cannot delete your own account.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.ID == id);
        }

        // Simple password hashing using SHA256
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}