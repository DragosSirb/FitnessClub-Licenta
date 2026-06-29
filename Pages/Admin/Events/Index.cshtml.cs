using System.ComponentModel.DataAnnotations;
using FitnessClub.Data;
using FitnessClub.Models;
using EventEntity = FitnessClub.Models.Event;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Admin.Events
{
    [Authorize(Roles = "admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<EventEntity> Events { get; set; } = new();

        [BindProperty]
        public EventInput Input { get; set; } = new();

        public class EventInput
        {
            [Required] public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            [Required] public DateTime Date { get; set; }
            [Required] public TimeOnly StartTime { get; set; }
            [Required] public TimeOnly EndTime { get; set; }
            [Required] public string Location { get; set; } = string.Empty;
            [Required, Range(0, 99999)] public decimal Price { get; set; }
            [Required, Range(1, 10000)] public int MaxParticipants { get; set; }
        }

        public async Task OnGetAsync()
        {
            Events = await _context.Events
                .Include(e => e.Enrollments)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }

            _context.Events.Add(new EventEntity
            {
                Name = Input.Name,
                Description = Input.Description,
                Date = Input.Date,
                StartTime = Input.StartTime,
                EndTime = Input.EndTime,
                Location = Input.Location,
                Price = Input.Price,
                MaxParticipants = Input.MaxParticipants,
                IsActive = true,
                Status = EventStatus.Upcoming
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Eveniment creat.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Enrollments)
                    .ThenInclude(e => e.Member)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev != null)
            {
                ev.Status = EventStatus.Cancelled;
                foreach (var enrollment in ev.Enrollments)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = enrollment.Member.UserId,
                        Title = "Eveniment anulat",
                        Message = $"Evenimentul '{ev.Name}' din {ev.Date:dd MMM yyyy} a fost anulat.",
                        Type = NotificationType.Event,
                        Link = "/Member/Events"
                    });
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Eveniment anulat și membrii notificați.";
            return RedirectToPage();
        }
    }
}
