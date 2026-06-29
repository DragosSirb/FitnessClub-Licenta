using System.ComponentModel.DataAnnotations;
using FitnessClub.Data;
using FitnessClub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Profile
{
    [Authorize]
    public class SetupModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SetupModel(UserManager<ApplicationUser> userManager, AppDbContext context, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            public string LastName { get; set; } = string.Empty;

            [Required]
            public DateTime DateOfBirth { get; set; }

            [Required]
            public string Gender { get; set; } = string.Empty;

            [Required]
            public string Phone { get; set; } = string.Empty;

            [Required]
            public string Address { get; set; } = string.Empty;

            public decimal? GoalWeight { get; set; }

            public IFormFile? ProfilePhoto { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });
            if (user.ProfileCompleted) return RedirectToPage("/Member/Dashboard");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
            if (member == null) return RedirectToPage("/Index");

            member.FirstName = Input.FirstName;
            member.LastName = Input.LastName;
            member.DateOfBirth = Input.DateOfBirth;
            member.Gender = Input.Gender;
            member.Phone = Input.Phone;
            member.Address = Input.Address;
            member.GoalWeight = Input.GoalWeight;

            if (Input.ProfilePhoto != null && Input.ProfilePhoto.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{user.Id}{Path.GetExtension(Input.ProfilePhoto.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await Input.ProfilePhoto.CopyToAsync(stream);
                member.ProfileImageUrl = $"/uploads/profiles/{fileName}";
            }

            user.ProfileCompleted = true;
            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            TempData["ProfileCompleted"] = true;
            return Page();
        }
    }
}
