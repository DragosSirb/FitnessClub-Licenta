using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using EventEntity = FitnessClub.Models.Event;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Member
{
    [Authorize(Roles = "member")]
    public class EventsModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventsModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<EventEntity> UpcomingEvents { get; set; } = new();
        public List<EventEnrollment> MyEnrollments { get; set; } = new();
        public Models.Member? CurrentMember { get; set; }

        [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "upcoming";

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            CurrentMember = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
            if (CurrentMember == null) return RedirectToPage("/Index");

            UpcomingEvents = await _context.Events
                .Include(e => e.Enrollments)
                .Where(e => e.IsActive && e.Status == EventStatus.Upcoming && e.Date >= DateTime.Today)
                .OrderBy(e => e.Date)
                .ToListAsync();

            if (Tab == "my" || Tab == "history")
            {
                MyEnrollments = await _context.EventEnrollments
                    .Include(e => e.Event)
                    .Where(e => e.MemberId == CurrentMember.Id)
                    .OrderByDescending(e => e.Event.Date)
                    .ToListAsync();
            }

            return Page();
        }
    }
}
