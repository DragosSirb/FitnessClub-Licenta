using System.ComponentModel.DataAnnotations;
using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrainerEntity = FitnessClub.Models.Trainer;

namespace FitnessClub.Pages.Admin.Classes
{
    [Authorize(Roles = "admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<GroupClass> Classes { get; set; } = new();
        public List<TrainerEntity> Trainers { get; set; } = new();

        [BindProperty]
        public ClassInput Input { get; set; } = new();

        [BindProperty]
        public ScheduleInput SchedInput { get; set; } = new();

        public class ClassInput
        {
            [Required] public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            [Required] public int TrainerId { get; set; }
            [Required, Range(15, 180)] public int DurationMinutes { get; set; } = 60;
            public bool IncludedInSubscription { get; set; } = true;
        }

        public class ScheduleInput
        {
            [Required] public int GroupClassId { get; set; }
            [Required] public DateTime Date { get; set; }
            [Required] public TimeOnly StartTime { get; set; }
            [Required] public string Location { get; set; } = string.Empty;
            [Required, Range(1, 100)] public int MaxParticipants { get; set; } = 20;
        }

        public async Task OnGetAsync()
        {
            Classes = await _context.GroupClasses
                .Include(g => g.Trainer)
                .Include(g => g.Schedules)
                .OrderBy(g => g.Name)
                .ToListAsync();

            Trainers = await _context.Trainers.Where(t => t.IsActive).ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateClassAsync()
        {
            if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }

            _context.GroupClasses.Add(new GroupClass
            {
                Name = Input.Name,
                Description = Input.Description,
                TrainerId = Input.TrainerId,
                DurationMinutes = Input.DurationMinutes,
                IncludedInSubscription = Input.IncludedInSubscription,
                IsActive = true
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Class created.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddScheduleAsync()
        {
            _context.GroupClassSchedules.Add(new GroupClassSchedule
            {
                GroupClassId = SchedInput.GroupClassId,
                Date = SchedInput.Date,
                StartTime = SchedInput.StartTime,
                Location = SchedInput.Location,
                MaxParticipants = SchedInput.MaxParticipants,
                Status = ScheduleStatus.Scheduled
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Schedule added.";
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
                        Title = "Class Cancelled",
                        Message = $"The class on {schedule.Date:dd MMM yyyy} at {schedule.StartTime} has been cancelled.",
                        Type = NotificationType.General
                    });
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Schedule cancelled and members notified.";
            return RedirectToPage();
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
                TempData["Success"] = "Schedule updated.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditClassAsync(int classId, string name, string description, int trainerId, int durationMinutes, bool includedInSubscription)
        {
            var cls = await _context.GroupClasses.FindAsync(classId);
            if (cls != null)
            {
                cls.Name = name;
                cls.Description = description;
                cls.TrainerId = trainerId;
                cls.DurationMinutes = durationMinutes;
                cls.IncludedInSubscription = includedInSubscription;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Class updated.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteClassAsync(int id)
        {
            var cls = await _context.GroupClasses.FindAsync(id);
            if (cls != null) { cls.IsActive = false; await _context.SaveChangesAsync(); }
            return RedirectToPage();
        }
    }
}
