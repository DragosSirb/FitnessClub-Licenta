using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Admin.Orders
{
    [Authorize(Roles = "admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Order> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Orders
                .Include(o => o.Member)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(StatusFilter) && Enum.TryParse<OrderStatus>(StatusFilter, out var status))
                query = query.Where(o => o.Status == status);

            Orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.Member)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
            {
                order.Status = status;

                var statusLabel = status switch
                {
                    OrderStatus.Processing => "being prepared",
                    OrderStatus.Delivered => "completed / delivered",
                    OrderStatus.Cancelled => "cancelled",
                    _ => status.ToString().ToLower()
                };

                _context.Notifications.Add(new Notification
                {
                    UserId = order.Member.UserId,
                    Title = "Order Updated",
                    Message = $"Your order #{order.Id} is now {statusLabel}.",
                    Type = NotificationType.Order,
                    Link = "/Member/Orders"
                });

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Order #{order.Id} updated to {statusLabel}.";
            }

            return RedirectToPage(new { statusFilter = StatusFilter });
        }

        public async Task<IActionResult> OnPostMarkPaidAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.PaymentStatus = PaymentStatus.Completed;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { statusFilter = StatusFilter });
        }
    }
}
