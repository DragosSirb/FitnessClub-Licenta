using FitnessClub.Data;
using FitnessClub.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Plans
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
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