using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Public
{
    public class ClassesModel : PageModel
    {
        private readonly AppDbContext _context;

        public ClassesModel(AppDbContext context)
        {
            _context = context;
        }

        public List<GroupClassSchedule> UpcomingSchedules { get; set; } = new();

        public async Task OnGetAsync()
        {
            UpcomingSchedules = await _context.GroupClassSchedules
                .Include(s => s.GroupClass)
                    .ThenInclude(g => g.Trainer)
                .Where(s => s.Date >= DateTime.Today && s.Status == ScheduleStatus.Scheduled)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .Take(50)
                .ToListAsync();

            var now = TimeOnly.FromDateTime(DateTime.Now);
            UpcomingSchedules = UpcomingSchedules
                .Where(s => s.Date.Date > DateTime.Today || s.StartTime > now)
                .Take(20)
                .ToList();
        }
    }
}
