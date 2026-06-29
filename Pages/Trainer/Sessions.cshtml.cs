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
    public class SessionsModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SessionsModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<BookedSession> Sessions { get; set; } = new();
        public Models.Trainer? CurrentTrainer { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Tab { get; set; } = "pending";

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            CurrentTrainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (CurrentTrainer == null) return RedirectToPage("/Index");

            var pastConfirmed = await _context.BookedSessions
                .Where(s => s.TrainerId == CurrentTrainer.Id && s.Status == SessionStatus.Confirmed && s.Date < DateTime.Today)
                .ToListAsync();
            foreach (var s in pastConfirmed) s.Status = SessionStatus.Completed;

            var pastPending = await _context.BookedSessions
                .Where(s => s.TrainerId == CurrentTrainer.Id && s.Status == SessionStatus.Pending && s.Date < DateTime.Today)
                .ToListAsync();
            foreach (var s in pastPending) s.Status = SessionStatus.Cancelled;

            if (pastConfirmed.Any() || pastPending.Any()) await _context.SaveChangesAsync();

            var status = Tab switch
            {
                "confirmed" => SessionStatus.Confirmed,
                "completed" => SessionStatus.Completed,
                "cancelled" => SessionStatus.Cancelled,
                _ => SessionStatus.Pending
            };

            Sessions = await _context.BookedSessions
                .Include(s => s.Member)
                .Where(s => s.TrainerId == CurrentTrainer.Id && s.Status == status)
                .OrderByDescending(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddNoteAsync(int sessionId, string note)
        {
            var session = await _context.BookedSessions.FindAsync(sessionId);
            if (session == null) return RedirectToPage();

            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == _userManager.GetUserId(User));
            if (trainer == null) return RedirectToPage();

            _context.TrainerNotes.Add(new TrainerNote
            {
                TrainerId = trainer.Id,
                MemberId = session.MemberId,
                Date = DateTime.UtcNow,
                Note = note
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Notă adăugată.";
            return RedirectToPage(new { tab = Tab });
        }
    }
}
