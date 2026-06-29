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
    public class ClassesModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClassesModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<GroupClassSchedule> UpcomingSchedules { get; set; } = new();
        public List<GroupClassEnrollment> MyEnrollments { get; set; } = new();
        public List<string> AvailableClassNames { get; set; } = new();
        public Models.Member? CurrentMember { get; set; }
        public MemberSubscription? ActiveSubscription { get; set; }

        [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "schedule";
        [BindProperty(SupportsGet = true)] public string? ClassFilter { get; set; }
        [BindProperty(SupportsGet = true)] public string? PriceFilter { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            CurrentMember = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
            if (CurrentMember == null) return RedirectToPage("/Index");

            ActiveSubscription = await _context.MemberSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.MemberId == CurrentMember.Id && s.Status == SubscriptionStatus.Active)
                .FirstOrDefaultAsync();

            AvailableClassNames = await _context.GroupClasses
                .Where(g => g.IsActive)
                .Select(g => g.Name)
                .OrderBy(n => n)
                .ToListAsync();

            var dbQuery = _context.GroupClassSchedules
                .Include(s => s.GroupClass)
                    .ThenInclude(g => g.Trainer)
                .Include(s => s.Enrollments)
                .Where(s => s.Date >= DateTime.Today && s.Status == ScheduleStatus.Scheduled);

            if (!string.IsNullOrEmpty(ClassFilter))
                dbQuery = dbQuery.Where(s => s.GroupClass.Name == ClassFilter);

            if (PriceFilter == "free")
                dbQuery = dbQuery.Where(s => s.GroupClass.IncludedInSubscription);
            else if (PriceFilter == "paid")
                dbQuery = dbQuery.Where(s => !s.GroupClass.IncludedInSubscription);

            var allSchedules = await dbQuery
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            var now = TimeOnly.FromDateTime(DateTime.Now);
            UpcomingSchedules = allSchedules
                .Where(s => s.Date.Date > DateTime.Today || s.StartTime > now)
                .ToList();

            if (Tab == "my" || Tab == "history")
            {
                MyEnrollments = await _context.GroupClassEnrollments
                    .Include(e => e.Schedule)
                        .ThenInclude(s => s.GroupClass)
                            .ThenInclude(g => g.Trainer)
                    .Where(e => e.MemberId == CurrentMember.Id && e.Status != EnrollmentStatus.Cancelled)
                    .OrderByDescending(e => e.Schedule.Date)
                    .ToListAsync();
            }

            return Page();
        }
    }
}
