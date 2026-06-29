using FitnessClub.Data;
using FitnessClub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Trainer
{
    [Authorize(Roles = "trainer")]
    public class ClientsModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientsModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<ClientInfo> Clients { get; set; } = new();
        public Models.Trainer? CurrentTrainer { get; set; }
        public int? SelectedMemberId { get; set; }
        public List<TrainerNote> Notes { get; set; } = new();

        public class ClientInfo
        {
            public Models.Member Member { get; set; } = null!;
            public DateTime LastSessionDate { get; set; }
            public int TotalSessions { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? memberId = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            CurrentTrainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (CurrentTrainer == null) return RedirectToPage("/Index");

            var sessions = await _context.BookedSessions
                .Include(s => s.Member)
                .Where(s => s.TrainerId == CurrentTrainer.Id)
                .ToListAsync();

            Clients = sessions
                .GroupBy(s => s.MemberId)
                .Select(g => new ClientInfo
                {
                    Member = g.First().Member,
                    LastSessionDate = g.Max(s => s.Date),
                    TotalSessions = g.Count()
                })
                .OrderByDescending(c => c.LastSessionDate)
                .ToList();

            if (memberId.HasValue)
            {
                SelectedMemberId = memberId;
                Notes = await _context.TrainerNotes
                    .Where(n => n.TrainerId == CurrentTrainer.Id && n.MemberId == memberId.Value)
                    .OrderByDescending(n => n.Date)
                    .ToListAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddNoteAsync(int memberId, string note)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage();

            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (trainer == null) return RedirectToPage();

            _context.TrainerNotes.Add(new TrainerNote
            {
                TrainerId = trainer.Id,
                MemberId = memberId,
                Date = DateTime.UtcNow,
                Note = note
            });

            await _context.SaveChangesAsync();
            return RedirectToPage(new { memberId });
        }
    }
}
