using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Trainer
{
    [Authorize(Roles = "trainer")]
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Models.Trainer? CurrentTrainer { get; set; }
        public List<BookedSession> TodaySessions { get; set; } = new();
        public List<BookedSession> PendingRequests { get; set; } = new();
        public int TotalClients { get; set; }
        public int SessionsThisMonth { get; set; }
        public int ClassesThisMonth { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            CurrentTrainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (CurrentTrainer == null) return RedirectToPage("/Index");

            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            TodaySessions = await _context.BookedSessions
                .Include(s => s.Member)
                .Where(s => s.TrainerId == CurrentTrainer.Id && s.Date == today && s.Status == SessionStatus.Confirmed)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            PendingRequests = await _context.BookedSessions
                .Include(s => s.Member)
                .Where(s => s.TrainerId == CurrentTrainer.Id && s.Status == SessionStatus.Pending)
                .OrderBy(s => s.Date)
                .ToListAsync();

            TotalClients = await _context.BookedSessions
                .Where(s => s.TrainerId == CurrentTrainer.Id)
                .Select(s => s.MemberId)
                .Distinct()
                .CountAsync();

            SessionsThisMonth = await _context.BookedSessions
                .CountAsync(s => s.TrainerId == CurrentTrainer.Id
                    && s.Date >= monthStart
                    && s.Status == SessionStatus.Completed);

            ClassesThisMonth = await _context.GroupClasses
                .Include(g => g.Schedules)
                .Where(g => g.TrainerId == CurrentTrainer.Id)
                .SelectMany(g => g.Schedules)
                .CountAsync(s => s.Date >= monthStart && s.Status == ScheduleStatus.Completed);

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int sessionId)
        {
            var session = await _context.BookedSessions.FindAsync(sessionId);
            if (session == null) return RedirectToPage();

            session.Status = SessionStatus.Confirmed;
            await _context.SaveChangesAsync();

            var member = await _context.Members
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == session.MemberId);

            if (member != null)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = member.UserId,
                    Title = "Ședință confirmată",
                    Message = $"Ședința ta din {session.Date:dd MMM yyyy} la ora {session.StartTime} a fost confirmată.",
                    Type = NotificationType.Session,
                    Link = "/Member/BookSession?tab=my"
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeclineAsync(int sessionId)
        {
            var session = await _context.BookedSessions.FindAsync(sessionId);
            if (session == null) return RedirectToPage();

            session.Status = SessionStatus.Cancelled;
            await _context.SaveChangesAsync();

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == session.MemberId);

            if (member != null)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = member.UserId,
                    Title = "Ședință refuzată",
                    Message = $"Ședința ta din {session.Date:dd MMM yyyy} la ora {session.StartTime} a fost refuzată de antrenor.",
                    Type = NotificationType.Session,
                    Link = "/Member/BookSession"
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
