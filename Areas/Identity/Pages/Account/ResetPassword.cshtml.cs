using FitnessClub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FitnessClub.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool Succeeded { get; set; }
        public bool InvalidLink { get; set; }

        public class InputModel
        {
            public string Code { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Parola este obligatorie.")]
            [MinLength(6, ErrorMessage = "Parola trebuie să aibă cel puțin 6 caractere.")]
            public string Password { get; set; } = string.Empty;

            [Compare("Password", ErrorMessage = "Parolele nu coincid.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public IActionResult OnGet(string? code, string? email)
        {
            if (code == null || email == null)
            {
                InvalidLink = true;
                return Page();
            }

            Input = new InputModel
            {
                Code = code,
                Email = email
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                Succeeded = true;
                return Page();
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Code));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, Input.Password);

            if (result.Succeeded)
            {
                Succeeded = true;
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
