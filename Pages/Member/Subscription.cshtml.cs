using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Member
{
    [Authorize(Roles = "member")]
    public class SubscriptionModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public MemberSubscription? ActiveSubscription { get; set; }
        public List<MemberSubscription> History { get; set; } = new();
        public List<SubscriptionPlan> Plans { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "current";

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
            if (member == null) return RedirectToPage("/Index");

            ActiveSubscription = await _context.MemberSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.MemberId == member.Id && s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            History = await _context.MemberSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.MemberId == member.Id && s.Status != SubscriptionStatus.Active)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            Plans = await _context.SubscriptionPlans
                .Where(p => p.IsActive && p.Type == SubscriptionPlanType.Subscription)
                .OrderBy(p => p.Price)
                .ToListAsync();

            return Page();
        }
    }
}
