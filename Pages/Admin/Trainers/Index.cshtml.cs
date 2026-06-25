using System.ComponentModel.DataAnnotations;
using FitnessClub.Data;
using FitnessClub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrainerEntity = FitnessClub.Models.Trainer;

namespace FitnessClub.Pages.Admin.Trainers
{
    [Authorize(Roles = "admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<TrainerEntity> Trainers { get; set; } = new();

        [BindProperty]
        public CreateTrainerInput Input { get; set; } = new();

        public class CreateTrainerInput
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            public string LastName { get; set; } = string.Empty;

            [Required]
            public string Expertise { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
            Trainers = await _context.Trainers
                .Include(t => t.User)
                .Include(t => t.BookedSessions)
                .Include(t => t.GroupClasses)
                .OrderBy(t => t.LastName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            if (await _userManager.FindByEmailAsync(Input.Email) != null)
            {
                ModelState.AddModelError(string.Empty, "An account with this email already exists.");
                await OnGetAsync();
                return Page();
            }

            var tempPassword = $"Trainer@{Guid.NewGuid().ToString()[..6]}1!";

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                ProfileCompleted = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, tempPassword);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                await OnGetAsync();
                return Page();
            }

            await _userManager.AddToRoleAsync(user, "trainer");

            var trainer = new TrainerEntity
            {
                UserId = user.Id,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Expertise = Input.Expertise,
                IsActive = true
            };

            _context.Trainers.Add(trainer);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Trainer account created. Temp password: {tempPassword} (share with trainer to set their own password on first login).";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int trainerId)
        {
            var trainer = await _context.Trainers.FindAsync(trainerId);
            if (trainer != null)
            {
                trainer.IsActive = !trainer.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
