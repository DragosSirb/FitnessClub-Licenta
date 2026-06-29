using FitnessClub.Data;
using FitnessClub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Filters
{
    public class NotificationFilter : IAsyncPageFilter
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationFilter(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(context.HttpContext.User);
                if (user != null)
                {
                    var count = await _context.Notifications
                        .CountAsync(n => n.UserId == user.Id && !n.IsRead);

                    if (context.HandlerInstance is PageModel page)
                        page.ViewData["UnreadNotifications"] = count;
                }
            }

            await next();
        }
    }
}
