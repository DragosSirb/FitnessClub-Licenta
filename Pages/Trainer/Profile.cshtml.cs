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
    public class ProfileModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProfileModel(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? CurrentPhotoUrl { get; set; }

        public class InputModel
        {
            [Required]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            public string LastName { get; set; } = string.Empty;

            [Required]
            public string Expertise { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            [Range(0, 50)]
            public int YearsOfExperience { get; set; }

            public IFormFile? ProfilePhoto { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (trainer == null) return RedirectToPage("/Index");

            Input = new InputModel
            {
                FirstName = trainer.FirstName,
                LastName = trainer.LastName,
                Expertise = trainer.Expertise,
                Description = trainer.Description,
                YearsOfExperience = trainer.YearsOfExperience
            };

            CurrentPhotoUrl = trainer.ProfileImageUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (trainer == null) return RedirectToPage("/Index");

            trainer.FirstName = Input.FirstName;
            trainer.LastName = Input.LastName;
            trainer.Expertise = Input.Expertise;
            trainer.Description = Input.Description;
            trainer.YearsOfExperience = Input.YearsOfExperience;

            if (Input.ProfilePhoto != null && Input.ProfilePhoto.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"trainer_{user.Id}{Path.GetExtension(Input.ProfilePhoto.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await Input.ProfilePhoto.CopyToAsync(stream);
                trainer.ProfileImageUrl = $"/uploads/profiles/{fileName}";
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Profil actualizat!";
            return RedirectToPage();
        }
    }
}
