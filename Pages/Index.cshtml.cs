using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrainerEntity = FitnessClub.Models.Trainer;

namespace FitnessClub.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<SubscriptionPlan> PopularPlans { get; set; } = new();
        public List<TrainerEntity> FeaturedTrainers { get; set; } = new();
        public List<GroupClass> PopularClasses { get; set; } = new();
        public List<Event> UpcomingEvents { get; set; } = new();

        public async Task OnGetAsync()
        {
            PopularPlans = await _context.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .Take(3)
                .ToListAsync();

            FeaturedTrainers = await _context.Trainers
                .Where(t => t.IsActive)
                .Take(3)
                .ToListAsync();

            PopularClasses = await _context.GroupClasses
                .Where(c => c.IsActive)
                .Include(c => c.Trainer)
                .Take(3)
                .ToListAsync();

            var nowTime = TimeOnly.FromDateTime(DateTime.Now);
            var rawEvents = await _context.Events
                .Where(e => e.IsActive && e.Status == EventStatus.Upcoming && e.Date >= DateTime.Today)
                .OrderBy(e => e.Date)
                .Take(10)
                .ToListAsync();
            UpcomingEvents = rawEvents
                .Where(e => e.Date.Date > DateTime.Today || e.StartTime > nowTime)
                .Take(3)
                .ToList();
        }
    }
}
