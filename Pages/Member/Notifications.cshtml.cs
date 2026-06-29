using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Member
{
    [Authorize]
    public class NotificationsModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Notification> Notifications { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string? TypeFilter { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var query = _context.Notifications.Where(n => n.UserId == user.Id);

            if (!string.IsNullOrEmpty(TypeFilter) && Enum.TryParse<NotificationType>(TypeFilter, out var type))
                query = query.Where(n => n.Type == type);

            Notifications = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();

            var unread = await _context.Notifications
                .Where(n => n.UserId == user.Id && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            if (unread.Any())
                await _context.SaveChangesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == user.Id);

            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { typeFilter = TypeFilter });
        }

        public async Task<IActionResult> OnPostDeleteAllAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage();

            var all = await _context.Notifications.Where(n => n.UserId == user.Id).ToListAsync();
            _context.Notifications.RemoveRange(all);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
