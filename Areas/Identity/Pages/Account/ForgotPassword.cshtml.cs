using FitnessClub.Models;
using FitnessClub.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FitnessClub.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, EmailService emailService, IConfiguration configuration)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool Sent { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Emailul este obligatoriu.")]
            [EmailAddress(ErrorMessage = "Adresă de email invalidă.")]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            var adminEmail = _configuration["Email:SenderEmail"]!;

            var html = $@"
<div style='font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:24px'>
  <h2 style='color:#22c55e;margin-bottom:4px'>FitnessClub</h2>
  <hr style='border:none;border-top:1px solid #e5e7eb;margin-bottom:24px'/>
  <p>Un utilizator a solicitat resetarea parolei.</p>
  <p><strong>Email:</strong> {Input.Email}</p>
  {(user != null ? $"<p><strong>Cont găsit:</strong> Da — {user.UserName}</p>" : "<p><strong>Cont găsit:</strong> Nu (emailul nu există în sistem)</p>")}
  <p>Contactează utilizatorul și ajută-l să își reseteze parola.</p>
  <hr style='border:none;border-top:1px solid #e5e7eb;margin-top:24px'/>
  <p style='color:#9ca3af;font-size:12px'>FitnessClub &mdash; notificare automată</p>
</div>";

            await _emailService.SendPaymentConfirmationAsync(
                adminEmail,
                "Admin FitnessClub",
                $"Cerere resetare parolă — {Input.Email}",
                html);

            Sent = true;
            return Page();
        }
    }
}
