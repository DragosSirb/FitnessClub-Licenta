using FitnessClub.Data;
using FitnessClub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Member
{
    [Authorize(Roles = "member")]
    public class PlansModel : PageModel
    {
        private readonly AppDbContext _context;

        public PlansModel(AppDbContext context)
        {
            _context = context;
        }

        public List<SubscriptionPlan> Plans { get; set; } = new();

        public async Task OnGetAsync()
        {
            Plans = await _context.SubscriptionPlans
                .Where(p => p.IsActive)
                .ToListAsync();
        }
    }
}