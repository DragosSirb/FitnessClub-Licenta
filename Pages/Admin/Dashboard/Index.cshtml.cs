using FitnessClub.Data;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Admin.Dashboard
{
    [Authorize(Roles = "admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public int TotalMembers { get; set; }
        public int ActiveSubscriptions { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int PendingOrders { get; set; }
        public List<(string Month, decimal Revenue)> RevenueChart { get; set; } = new();
        public List<RecentActivity> RecentActivities { get; set; } = new();
        public List<string> LowStockAlerts { get; set; } = new();

        public class RecentActivity
        {
            public string Description { get; set; } = string.Empty;
            public DateTime Date { get; set; }
        }

        public async Task OnGetAsync()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            TotalMembers = await _context.Members.CountAsync(m => m.IsActive);

            ActiveSubscriptions = await _context.MemberSubscriptions
                .CountAsync(s => s.Status == SubscriptionStatus.Active);

            var subscriptionRevenue = await _context.MemberSubscriptions
                .Where(s => s.StartDate >= monthStart && s.PaymentStatus == PaymentStatus.Completed)
                .SumAsync(s => s.PricePaid);

            var orderRevenue = await _context.Orders
                .Where(o => o.CreatedAt >= monthStart && o.PaymentStatus == PaymentStatus.Completed)
                .SumAsync(o => o.TotalAmount);

            RevenueThisMonth = subscriptionRevenue + orderRevenue;

            PendingOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Pending);

            LowStockAlerts = await _context.Products
                .Where(p => p.IsActive && p.Stock < 5)
                .Select(p => $"{p.Name} ({p.Stock} left)")
                .ToListAsync();

            for (int i = 5; i >= 0; i--)
            {
                var m = monthStart.AddMonths(-i);
                var mEnd = m.AddMonths(1);
                var rev = await _context.MemberSubscriptions
                    .Where(s => s.StartDate >= m && s.StartDate < mEnd && s.PaymentStatus == PaymentStatus.Completed)
                    .SumAsync(s => s.PricePaid);
                rev += await _context.Orders
                    .Where(o => o.CreatedAt >= m && o.CreatedAt < mEnd && o.PaymentStatus == PaymentStatus.Completed)
                    .SumAsync(o => o.TotalAmount);
                RevenueChart.Add((m.ToString("MMM"), rev));
            }

            var recentSubs = await _context.MemberSubscriptions
                .Include(s => s.Member)
                .Include(s => s.Plan)
                .OrderByDescending(s => s.StartDate)
                .Take(5)
                .Select(s => new RecentActivity
                {
                    Description = $"{s.Member.FirstName} {s.Member.LastName} subscribed to {s.Plan.Name}",
                    Date = s.StartDate
                })
                .ToListAsync();

            var recentOrders = await _context.Orders
                .Include(o => o.Member)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new RecentActivity
                {
                    Description = $"{o.Member.FirstName} {o.Member.LastName} placed an order ({o.TotalAmount} RON)",
                    Date = o.CreatedAt
                })
                .ToListAsync();

            RecentActivities = recentSubs.Concat(recentOrders)
                .OrderByDescending(a => a.Date)
                .Take(8)
                .ToList();
        }
    }
}
