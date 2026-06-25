using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Admin.Members
{
    [Authorize(Roles = "admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<MemberDetail> Members { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public class MemberDetail
        {
            public Models.Member Member { get; set; } = null!;
            public MemberSubscription? ActiveSubscription { get; set; }
            public int TotalSessions { get; set; }
            public int TotalOrders { get; set; }
        }

        public async Task OnGetAsync()
        {
            var query = _context.Members
                .Include(m => m.User)
                .Include(m => m.Subscriptions)
                    .ThenInclude(s => s.Plan)
                .Include(m => m.BookedSessions)
                .Include(m => m.Orders)
                .AsQueryable();

            if (!string.IsNullOrEmpty(Search))
            {
                var s = Search.ToLower();
                query = query.Where(m =>
                    m.FirstName.ToLower().Contains(s) ||
                    m.LastName.ToLower().Contains(s) ||
                    m.User.Email!.ToLower().Contains(s));
            }

            var members = await query.OrderBy(m => m.LastName).ToListAsync();

            Members = members.Select(m => new MemberDetail
            {
                Member = m,
                ActiveSubscription = m.Subscriptions
                    .FirstOrDefault(s => s.Status == SubscriptionStatus.Active),
                TotalSessions = m.BookedSessions.Count,
                TotalOrders = m.Orders.Count
            }).ToList();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int memberId)
        {
            var member = await _context.Members.FindAsync(memberId);
            if (member != null)
            {
                member.IsActive = !member.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
