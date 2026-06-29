using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Admin.Payments
{
    [Authorize(Roles = "admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<PaymentRecord> Records { get; set; } = new();

        public class PaymentRecord
        {
            public string Type { get; set; } = string.Empty;
            public string MemberName { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string PaymentMethod { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime Date { get; set; }
        }

        public async Task OnGetAsync()
        {
            var subscriptions = await _context.MemberSubscriptions
                .Include(s => s.Member)
                .Include(s => s.Plan)
                .OrderByDescending(s => s.StartDate)
                .Take(50)
                .Select(s => new PaymentRecord
                {
                    Type = $"Abonament - {s.Plan.Name}",
                    MemberName = s.Member.FirstName + " " + s.Member.LastName,
                    Amount = s.PricePaid,
                    PaymentMethod = s.PaymentMethod == PaymentMethod.Online ? "Online" : "La sală",
                    Status = s.PaymentStatus.ToString(),
                    Date = s.StartDate
                })
                .ToListAsync();

            var orders = await _context.Orders
                .Include(o => o.Member)
                .OrderByDescending(o => o.CreatedAt)
                .Take(50)
                .Select(o => new PaymentRecord
                {
                    Type = $"Order #{o.Id}",
                    MemberName = o.Member.FirstName + " " + o.Member.LastName,
                    Amount = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod == PaymentMethod.Online ? "Online" : "La sală",
                    Status = o.PaymentStatus.ToString(),
                    Date = o.CreatedAt
                })
                .ToListAsync();

            var sessions = await _context.BookedSessions
                .Include(s => s.Member)
                .OrderByDescending(s => s.Date)
                .Take(50)
                .Select(s => new PaymentRecord
                {
                    Type = "Sesiune",
                    MemberName = s.Member.FirstName + " " + s.Member.LastName,
                    Amount = s.PricePaid,
                    PaymentMethod = s.PaymentMethod == PaymentMethod.Online ? "Online" : "La sală",
                    Status = s.PaymentStatus.ToString(),
                    Date = s.Date
                })
                .ToListAsync();

            Records = subscriptions.Concat(orders).Concat(sessions)
                .OrderByDescending(r => r.Date)
                .ToList();
        }
    }
}
