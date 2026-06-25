using System.ComponentModel.DataAnnotations;
using FitnessClub.Data;
using FitnessClub.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid) return Page();

            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "admin"))
                        return RedirectToPage("/Admin/Dashboard/Index");

                    if (await _userManager.IsInRoleAsync(user, "trainer"))
                    {
                        var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == user.Id);
                        if (trainer != null && !trainer.IsActive)
                        {
                            await _signInManager.SignOutAsync();
                            ModelState.AddModelError(string.Empty, "Contul tău a fost dezactivat temporar. Te rugăm să contactezi sala pentru asistență.");
                            return Page();
                        }
                        return RedirectToPage("/Trainer/Dashboard");
                    }

                    var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
                    if (member != null && !member.IsActive)
                    {
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Contul tău a fost dezactivat temporar. Te rugăm să contactezi sala pentru asistență.");
                        return Page();
                    }

                    if (!user.ProfileCompleted)
                        return RedirectToPage("/Profile/Setup");
                }
                return RedirectToPage("/Member/Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Email sau parolă incorectă.");
            return Page();
        }
    }
}
