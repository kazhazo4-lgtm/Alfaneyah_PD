using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProjectsDashboards.Models;
using System.Globalization;

namespace ProjectsDashboards.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // This is the entry point - handles role-based redirects
        public IActionResult Index()
        {
            // Check user role and redirect accordingly
            if (User.IsInRole("Accountant") || User.IsInRole("Staff"))
            {
                // Accountants and Staff go to Projects
                return RedirectToAction("Index", "Projects");
            }

            // Owners go to Dashboard
            return RedirectToAction("Dashboard");
        }

        // Dashboard action - only accessible to Owners (implicitly via redirect logic)
        public async Task<IActionResult> Dashboard(int? projectId)
        {
            // Double-check authorization - if Accountant or Staff somehow access this, redirect them
            if (User.IsInRole("Accountant") || User.IsInRole("Staff"))
            {
                return RedirectToAction("Index", "Projects");
            }

            var viewModel = new DashboardViewModel();

            // Get all projects with related data
            var projects = await _context.Projects
                .Include(p => p.PaymentClaims)
                .Include(p => p.VariationOrders)
                .Include(p => p.CreatedByUser)
                .ToListAsync();

            viewModel.AllProjects = projects;

            if (projectId.HasValue)
            {
                viewModel.SelectedProject = projects.FirstOrDefault(p => p.Id == projectId);
            }

            // Calculate summary statistics
            viewModel.TotalProjects = projects.Count;
            viewModel.CompletedProjects = projects.Count(p => p.ProgressPercentage >= 100);
            viewModel.InProgressProjects = projects.Count(p => p.ProgressPercentage > 0 && p.ProgressPercentage < 100);
            viewModel.NotStartedProjects = projects.Count(p => p.ProgressPercentage == 0);

            // Financial statistics
            viewModel.TotalContractValue = projects.Sum(p => p.ContractValue ?? 0);
            viewModel.TotalVOs = projects.Sum(p => p.TotalVOs ?? 0);
            viewModel.TotalClaims = projects.Sum(p => p.TotalClaims ?? 0);
            viewModel.TotalRevisedValue = viewModel.TotalContractValue + viewModel.TotalVOs;

            // Project Status Chart Data
            viewModel.ProjectStatusData = new List<ChartData>
            {
                new ChartData { Label = "Completed", Value = viewModel.CompletedProjects, Color = "#28a745" },
                new ChartData { Label = "In Progress", Value = viewModel.InProgressProjects, Color = "#ffc107" },
                new ChartData { Label = "Not Started", Value = viewModel.NotStartedProjects, Color = "#dc3545" }
            };

            // Top Projects by Value
            viewModel.TopProjectsByValue = projects
                .Where(p => p.RevisedContractValue.HasValue)
                .OrderByDescending(p => p.RevisedContractValue)
                .Take(5)
                .Select(p => new ChartData
                {
                    Label = p.ProjectName ?? "Unnamed Project",
                    Value = p.RevisedContractValue ?? 0,
                    Color = GetRandomColor()
                })
                .ToList();

            // ===== FIXED: Monthly Progress with proper date handling for Arabic culture =====
            var allClaims = projects.SelectMany(p => p.PaymentClaims ?? new List<PaymentClaim>()).ToList();
            var allVOs = projects.SelectMany(p => p.VariationOrders ?? new List<VariationOrder>()).ToList();

            // Create a dictionary to store monthly data using numeric key (yyyy-MM)
            var monthlyData = new Dictionary<string, MonthlyProgressData>();

            // Process claims
            foreach (var claim in allClaims)
            {
                string monthKey = claim.ClaimDate.ToString("yyyy-MM"); // This gives "2026-03"

                if (!monthlyData.ContainsKey(monthKey))
                {
                    monthlyData[monthKey] = new MonthlyProgressData
                    {
                        Claims = 0,
                        VOs = 0
                    };
                }
                monthlyData[monthKey].Claims += claim.ClaimAmount;
            }

            // Process VOs
            foreach (var vo in allVOs)
            {
                string monthKey = vo.ApprovedDate.ToString("yyyy-MM"); // This gives "2026-03"

                if (!monthlyData.ContainsKey(monthKey))
                {
                    monthlyData[monthKey] = new MonthlyProgressData
                    {
                        Claims = 0,
                        VOs = 0
                    };
                }
                monthlyData[monthKey].VOs += vo.VOAmount;
            }

            // Convert to sorted list with proper month names in English
            viewModel.MonthlyProgress = monthlyData
                .OrderBy(m => m.Key) // Sort by "2026-03", "2026-04", etc.
                .Select(m => new MonthlyProgressData
                {
                    Month = DateTime.ParseExact(m.Key + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture)
                        .ToString("MMM yyyy", CultureInfo.InvariantCulture), // This gives "Mar 2026" in English
                    MonthKey = m.Key,
                    Claims = m.Value.Claims,
                    VOs = m.Value.VOs
                })
                .ToList();

            return View(viewModel);
        }

        private string GetRandomColor()
        {
            var random = new Random();
            var colors = new[] { "#007bff", "#28a745", "#ffc107", "#17a2b8", "#6f42c1", "#fd7e14", "#e83e8c" };
            return colors[random.Next(colors.Length)];
        }
    }
}