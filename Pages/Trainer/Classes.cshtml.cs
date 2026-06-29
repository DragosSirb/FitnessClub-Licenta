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
    public class ClassesModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClassesModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<GroupClass> MyClasses { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (trainer == null) return RedirectToPage("/Index");

            MyClasses = await _context.GroupClasses
                .Include(g => g.Schedules)
                    .ThenInclude(s => s.Enrollments)
                .Where(g => g.TrainerId == trainer.Id && g.IsActive)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostEditScheduleAsync(int scheduleId, DateTime date, TimeOnly startTime, string location, int maxParticipants)
        {
            var schedule = await _context.GroupClassSchedules.FindAsync(scheduleId);
            if (schedule != null)
            {
                schedule.Date = date;
                schedule.StartTime = startTime;
                schedule.Location = location;
                schedule.MaxParticipants = maxParticipants;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Program actualizat.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelScheduleAsync(int scheduleId)
        {
            var schedule = await _context.GroupClassSchedules
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Member)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule != null)
            {
                schedule.Status = ScheduleStatus.Cancelled;
                foreach (var enrollment in schedule.Enrollments)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = enrollment.Member.UserId,
                        Title = "Clasă anulată",
                        Message = $"Clasa din {schedule.Date:dd MMM yyyy} la ora {schedule.StartTime} a fost anulată.",
                        Type = NotificationType.General
                    });
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = "Program anulat și membrii notificați.";
            }
            return RedirectToPage();
        }
    }
}
