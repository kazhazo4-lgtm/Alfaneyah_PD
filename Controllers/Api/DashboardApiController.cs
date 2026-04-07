using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectsDashboards.Models;

namespace ProjectsDashboards.Controllers.Api
{
    public class DashboardApiController : Controller
    {
        [Route("api/[controller]")]
        [ApiController]
        [Authorize]
        public class DashboardController : ControllerBase
        {
            private readonly ApplicationDbContext _context;

            public DashboardController(ApplicationDbContext context)
            {
                _context = context;
            }

            [HttpGet("projects")]
            public async Task<IActionResult> GetProjects()
            {
                var projects = await _context.Projects
                    .Include(p => p.PaymentClaims)
                    .Include(p => p.VariationOrders)
                    .Select(p => new
                    {
                        p.Id,
                        p.ProjectName,
                        ContractValue = p.ContractValue ?? 0m,
                        TotalVOs = p.VariationOrders.Sum(v => v.VOAmount),
                        TotalClaims = p.PaymentClaims.Sum(pc => pc.ClaimAmount)
                    })
                    .Select(p => new
                    {
                        p.Id,
                        p.ProjectName,
                        // Calculate RevisedValue inline
                        ProgressPercentage = (p.ContractValue + p.TotalVOs) > 0
                            ? Math.Round((p.TotalClaims / (p.ContractValue + p.TotalVOs)) * 100, 2)
                            : 0m
                    })
                    .ToListAsync();

                return Ok(projects);
            }
        }
    }
}
