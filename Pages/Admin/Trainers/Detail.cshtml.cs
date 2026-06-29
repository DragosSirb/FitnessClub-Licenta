using System.ComponentModel.DataAnnotations;
using FitnessClub.Data;
using FitnessClub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrainerEntity = FitnessClub.Models.Trainer;

namespace FitnessClub.Pages.Admin.Trainers
{
    [Authorize(Roles = "admin")]
    public class DetailModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailModel(AppDbContext context)
        {
            _context = context;
        }

        public TrainerEntity Trainer { get; set; } = null!;

        [BindProperty]
        public EditInput Input { get; set; } = new();

        public class EditInput
        {
            [Required] public string FirstName { get; set; } = string.Empty;
            [Required] public string LastName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            [Required] public string Expertise { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            [Range(0, 50)] public int YearsOfExperience { get; set; }
            [Range(0, 10000)] public decimal SessionPrice { get; set; } = 80;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.User)
                .Include(t => t.BookedSessions).ThenInclude(s => s.Member)
                .Include(t => t.GroupClasses).ThenInclude(g => g.Schedules)
                .Include(t => t.Notes).ThenInclude(n => n.Member)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null) return RedirectToPage("/Admin/Trainers/Index");

            Trainer = trainer;
            Input = new EditInput
            {
                FirstName = trainer.FirstName,
                LastName = trainer.LastName,
                Phone = trainer.Phone,
                Expertise = trainer.Expertise,
                Description = trainer.Description,
                YearsOfExperience = trainer.YearsOfExperience,
                SessionPrice = trainer.SessionPrice
            };

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null) return RedirectToPage("/Admin/Trainers/Index");

            trainer.FirstName = Input.FirstName;
            trainer.LastName = Input.LastName;
            trainer.Phone = Input.Phone;
            trainer.Expertise = Input.Expertise;
            trainer.Description = Input.Description;
            trainer.YearsOfExperience = Input.YearsOfExperience;
            trainer.SessionPrice = Input.SessionPrice;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Detalii antrenor salvate.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
                trainer.IsActive = !trainer.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { id });
        }
    }
}
