using System.ComponentModel.DataAnnotations;
using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Admin.Notifications
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

        public List<Models.Member> Members { get; set; } = new();

        [BindProperty]
        public NotificationInput Input { get; set; } = new();

        public class NotificationInput
        {
            [Required]
            public string Title { get; set; } = string.Empty;

            [Required]
            public string Message { get; set; } = string.Empty;

            public int? MemberId { get; set; }
        }

        public async Task OnGetAsync()
        {
            Members = await _context.Members
                .Include(m => m.User)
                .Where(m => m.IsActive)
                .OrderBy(m => m.LastName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostSendAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            if (Input.MemberId.HasValue)
            {
                var member = await _context.Members.FindAsync(Input.MemberId.Value);
                if (member != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = member.UserId,
                        Title = Input.Title,
                        Message = Input.Message,
                        Type = NotificationType.General,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            else
            {
                var members = await _context.Members.Where(m => m.IsActive).ToListAsync();
                foreach (var member in members)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = member.UserId,
                        Title = Input.Title,
                        Message = Input.Message,
                        Type = NotificationType.General,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = Input.MemberId.HasValue
                ? "Notificare trimisă membrului."
                : "Notificare trimisă tuturor membrilor.";

            return RedirectToPage();
        }
    }
}
