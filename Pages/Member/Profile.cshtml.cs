using System.ComponentModel.DataAnnotations;
using FitnessClub.Data;
using FitnessClub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Member
{
    [Authorize(Roles = "member")]
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

        [BindProperty]
        public PasswordInputModel PasswordInput { get; set; } = new();

        public string? CurrentPhotoUrl { get; set; }

        public class InputModel
        {
            [Required]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            public string LastName { get; set; } = string.Empty;

            [Required]
            public string Phone { get; set; } = string.Empty;

            [Required]
            public string Address { get; set; } = string.Empty;

            public decimal? GoalWeight { get; set; }

            public IFormFile? ProfilePhoto { get; set; }
        }

        public class PasswordInputModel
        {
            public string? CurrentPassword { get; set; }
            public string? NewPassword { get; set; }
            public string? ConfirmNewPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
            if (member == null) return RedirectToPage("/Index");

            Input = new InputModel
            {
                FirstName = member.FirstName,
                LastName = member.LastName,
                Phone = member.Phone,
                Address = member.Address,
                GoalWeight = member.GoalWeight
            };

            CurrentPhotoUrl = member.ProfileImageUrl;
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

            await _context.SaveChangesAsync();
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            if (string.IsNullOrEmpty(PasswordInput.CurrentPassword) ||
                string.IsNullOrEmpty(PasswordInput.NewPassword) ||
                PasswordInput.NewPassword != PasswordInput.ConfirmNewPassword)
            {
                TempData["PasswordError"] = "Please fill all password fields and make sure they match.";
                return RedirectToPage();
            }

            var result = await _userManager.ChangePasswordAsync(user, PasswordInput.CurrentPassword, PasswordInput.NewPassword);
            if (result.Succeeded)
                TempData["PasswordSuccess"] = "Password changed successfully!";
            else
                TempData["PasswordError"] = string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToPage();
        }
    }
}
