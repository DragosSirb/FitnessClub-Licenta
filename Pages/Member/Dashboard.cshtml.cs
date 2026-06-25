using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MemberEntity = FitnessClub.Models.Member;

namespace FitnessClub.Pages.Members
{
    [Authorize(Roles = "member")]
    public class DashboardModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private static readonly Random RandomGenerator = new();

        public DashboardModel(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public MemberEntity? CurrentMember { get; set; }
        public MemberSubscription? ActiveSubscription { get; set; }
        public List<BookedSession> UpcomingSessions { get; set; } = new();
        public Quote? RandomQuote { get; set; }
        public int TotalSessions { get; set; }
        public int TotalClasses { get; set; }
        public bool HasPendingPayments { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            if (!user.ProfileCompleted)
                return RedirectToPage("/Profile/Setup");

            CurrentMember = await GetMember(user.Id);
            if (CurrentMember == null)
                return RedirectToPage("/Index");

            var today = DateTime.Today;

            ActiveSubscription = await GetActiveSubscription(CurrentMember.Id);
            UpcomingSessions = await GetUpcomingSessions(CurrentMember.Id, today);
            TotalSessions = await GetTotalSessions(CurrentMember.Id);
            TotalClasses = await GetTotalClasses(CurrentMember.Id);
            HasPendingPayments = await HasPendingPaymentsAsync(CurrentMember.Id);
            RandomQuote = await GetRandomQuote();

            return Page();
        }

        private async Task<MemberEntity?> GetMember(string userId)
        {
            return await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == userId);
        }

        private async Task<MemberSubscription?> GetActiveSubscription(int memberId)
        {
            return await _context.MemberSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.MemberId == memberId && s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
        }

        private async Task<List<BookedSession>> GetUpcomingSessions(int memberId, DateTime today)
        {
            return await _context.BookedSessions
                .Include(s => s.Trainer)
                .Where(s => s.MemberId == memberId
                    && s.Date >= today
                    && s.Status != SessionStatus.Cancelled)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .Take(5)
                .ToListAsync();
        }

        private async Task<int> GetTotalSessions(int memberId)
        {
            return await _context.BookedSessions
                .CountAsync(s => s.MemberId == memberId &&
                    (s.Status == SessionStatus.Completed ||
                     (s.Date < DateTime.Today && s.Status == SessionStatus.Confirmed)));
        }

        private async Task<int> GetTotalClasses(int memberId)
        {
            return await _context.GroupClassEnrollments
                .CountAsync(e => e.MemberId == memberId && e.Status == EnrollmentStatus.Confirmed);
        }

        private async Task<bool> HasPendingPaymentsAsync(int memberId)
        {
            return await _context.BookedSessions
                       .AnyAsync(s => s.MemberId == memberId && s.PaymentStatus == PaymentStatus.Pending)
                   || await _context.MemberSubscriptions
                       .AnyAsync(s => s.MemberId == memberId && s.PaymentStatus == PaymentStatus.Pending);
        }

        private async Task<Quote?> GetRandomQuote()
        {
            var quotes = await _context.Quotes
                .Where(q => q.IsActive)
                .ToListAsync();

            if (!quotes.Any())
                return null;

            return quotes[RandomGenerator.Next(quotes.Count)];
        }
    }
}