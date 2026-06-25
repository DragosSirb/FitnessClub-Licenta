using System.ComponentModel.DataAnnotations;
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
    public class AvailabilityModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AvailabilityModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Models.Trainer? CurrentTrainer { get; set; }
        public List<TrainerAvailability> Availabilities { get; set; } = new();
        public List<TrainerUnavailability> Unavailabilities { get; set; } = new();

        [BindProperty]
        public AvailabilityInput AvailInput { get; set; } = new();

        [BindProperty]
        public UnavailabilityInput UnavailInput { get; set; } = new();

        public class AvailabilityInput
        {
            [Required]
            public DayOfWeek DayOfWeek { get; set; }

            [Required]
            public TimeOnly StartTime { get; set; }

            [Required]
            public TimeOnly EndTime { get; set; }
        }

        public class UnavailabilityInput
        {
            [Required]
            public DateTime Date { get; set; }

            public string? Reason { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            CurrentTrainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (CurrentTrainer == null) return RedirectToPage("/Index");

            Availabilities = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == CurrentTrainer.Id)
                .OrderBy(a => a.DayOfWeek)
                .ToListAsync();

            Unavailabilities = await _context.TrainerUnavailabilities
                .Where(u => u.TrainerId == CurrentTrainer.Id && u.Date >= DateTime.Today)
                .OrderBy(u => u.Date)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAvailabilityAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == user!.Id);
            if (trainer == null) return RedirectToPage();

            var existing = await _context.TrainerAvailabilities
                .FirstOrDefaultAsync(a => a.TrainerId == trainer.Id && a.DayOfWeek == AvailInput.DayOfWeek);

            if (existing != null)
            {
                existing.StartTime = AvailInput.StartTime;
                existing.EndTime = AvailInput.EndTime;
            }
            else
            {
                _context.TrainerAvailabilities.Add(new TrainerAvailability
                {
                    TrainerId = trainer.Id,
                    DayOfWeek = AvailInput.DayOfWeek,
                    StartTime = AvailInput.StartTime,
                    EndTime = AvailInput.EndTime
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Availability saved.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAvailabilityAsync(int id)
        {
            var a = await _context.TrainerAvailabilities.FindAsync(id);
            if (a != null) _context.TrainerAvailabilities.Remove(a);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddUnavailabilityAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == user!.Id);
            if (trainer == null) return RedirectToPage();

            _context.TrainerUnavailabilities.Add(new TrainerUnavailability
            {
                TrainerId = trainer.Id,
                Date = UnavailInput.Date,
                Reason = UnavailInput.Reason
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Unavailable date added.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteUnavailabilityAsync(int id)
        {
            var u = await _context.TrainerUnavailabilities.FindAsync(id);
            if (u != null) _context.TrainerUnavailabilities.Remove(u);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
