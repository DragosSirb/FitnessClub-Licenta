using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Public
{
    public class EventsModel : PageModel
    {
        private readonly AppDbContext _context;

        public EventsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Event> Events { get; set; } = new();

        public async Task OnGetAsync()
        {
            Events = await _context.Events
                .Include(e => e.Enrollments)
                .Where(e => e.IsActive && e.Status == EventStatus.Upcoming && e.Date >= DateTime.Today)
                .OrderBy(e => e.Date)
                .ToListAsync();
        }
    }
}
