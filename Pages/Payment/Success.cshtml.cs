using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using FitnessClub.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Text.Json;
using PaymentMethod = FitnessClub.Models.Enums.PaymentMethod;

namespace FitnessClub.Pages.Payment
{
    public class SuccessModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public SuccessModel(AppDbContext context, IConfiguration configuration, EmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public string? PaymentType { get; set; }
        public bool Failed { get; set; }

        public async Task OnGetAsync(string? type, string? payment_intent)
        {
            PaymentType = type;

            if (payment_intent == null)
                return;

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            var alreadyProcessed = await _context.Payments
                .AnyAsync(p => p.TransactionId == payment_intent);

            if (alreadyProcessed)
                return;

            var service = new PaymentIntentService();
            var intent = await service.GetAsync(payment_intent);

            if (intent.Status != "succeeded")
            {
                Failed = true;
                return;
            }

            var meta = intent.Metadata;

            switch (type)
            {
                case "order":
                    await HandleOrder(meta, intent.Amount);
                    break;
                case "subscription":
                    await HandleSubscription(meta, intent.Amount);
                    break;
                case "session":
                    await HandleSession(meta, intent.Amount);
                    break;
                case "class":
                    await HandleClass(meta, intent.Amount);
                    break;
                case "event":
                    await HandleEvent(meta, intent.Amount);
                    break;
            }

            HttpContext.Session.Remove("cart");
        }

        private async Task<(string email, string name)> GetMemberContact(int memberId)
        {
            var member = await _context.Members
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == memberId);
            if (member == null) return ("", "");
            return (member.User.Email ?? "", $"{member.FirstName} {member.LastName}");
        }

        private static string EmailTemplate(string name, string body) => $@"
<div style='font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:24px'>
  <h2 style='color:#22c55e;margin-bottom:4px'>FitnessClub</h2>
  <hr style='border:none;border-top:1px solid #e5e7eb;margin-bottom:24px'/>
  <p>Bună, <strong>{name}</strong>!</p>
  {body}
  <hr style='border:none;border-top:1px solid #e5e7eb;margin-top:24px'/>
  <p style='color:#9ca3af;font-size:12px'>FitnessClub &mdash; echipa ta de fitness</p>
</div>";

        private async Task HandleOrder(IDictionary<string, string> meta, long amountCents)
        {
            var memberId = int.Parse(meta["member_id"]);
            var cartJson = meta["cart"];
            var cart = JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson) ?? new();

            var productIds = cart.Keys.ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

            var order = new Order
            {
                MemberId = memberId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                PaymentMethod = PaymentMethod.Online,
                PaymentStatus = PaymentStatus.Completed,
                TotalAmount = amountCents / 100m
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var product in products)
            {
                if (!cart.TryGetValue(product.Id, out var qty)) continue;
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    Quantity = qty,
                    PriceAtPurchase = product.Price
                });
                product.Stock -= qty;
            }

            _context.Payments.Add(new Models.Payment
            {
                EntityType = PaymentEntityType.Order,
                EntityId = order.Id,
                Amount = order.TotalAmount,
                TransactionId = Request.Query["payment_intent"],
                Status = PaymentStatus.Completed,
                PaidAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var (email, name) = await GetMemberContact(memberId);
            var itemsHtml = string.Join("", products.Select(p =>
                $"<tr><td>{p.Name}</td><td style='text-align:right'>{cart[p.Id]} x {p.Price} RON</td></tr>"));
            await _emailService.SendPaymentConfirmationAsync(email, name,
                $"Confirmare comandă #{order.Id} — FitnessClub",
                EmailTemplate(name, $@"
<p>Comanda ta <strong>#{order.Id}</strong> a fost plasată cu succes.</p>
<table style='width:100%;border-collapse:collapse;margin:16px 0'>
  {itemsHtml}
  <tr style='font-weight:bold;border-top:1px solid #e5e7eb'>
    <td>Total</td><td style='text-align:right'>{order.TotalAmount} RON</td>
  </tr>
</table>
<p>Comanda va fi procesată în curând. Poți urmări statusul în contul tău.</p>"));
        }

        private async Task HandleSubscription(IDictionary<string, string> meta, long amountCents)
        {
            var memberId = int.Parse(meta["member_id"]);
            var planId = int.Parse(meta["plan_id"]);
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null) return;

            var subscription = new MemberSubscription
            {
                MemberId = memberId,
                PlanId = planId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(plan.DurationDays),
                Status = SubscriptionStatus.Active,
                PaymentMethod = PaymentMethod.Online,
                PaymentStatus = PaymentStatus.Completed,
                PricePaid = plan.Price
            };

            _context.MemberSubscriptions.Add(subscription);

            _context.Payments.Add(new Models.Payment
            {
                EntityType = PaymentEntityType.Subscription,
                EntityId = memberId,
                Amount = amountCents / 100m,
                TransactionId = Request.Query["payment_intent"],
                Status = PaymentStatus.Completed,
                PaidAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var (email, name) = await GetMemberContact(memberId);
            await _emailService.SendPaymentConfirmationAsync(email, name,
                $"Abonament activat — FitnessClub",
                EmailTemplate(name, $@"
<p>Abonamentul tău <strong>{plan.Name}</strong> a fost activat cu succes!</p>
<p>Valabil până la: <strong>{subscription.EndDate:dd MMMM yyyy}</strong></p>
<p>Suma achitată: <strong>{plan.Price} RON</strong></p>"));
        }

        private async Task HandleSession(IDictionary<string, string> meta, long amountCents)
        {
            var memberId = int.Parse(meta["member_id"]);
            var trainerId = int.Parse(meta["trainer_id"]);
            var date = DateTime.Parse(meta["date"]);
            var startTime = TimeOnly.Parse(meta["start_time"]);
            var notes = meta.ContainsKey("notes") ? meta["notes"] : "";
            var price = decimal.Parse(meta["price"]);

            var trainer = await _context.Trainers.FindAsync(trainerId);

            var session = new BookedSession
            {
                MemberId = memberId,
                TrainerId = trainerId,
                Date = date,
                StartTime = startTime,
                DurationMinutes = 60,
                PricePaid = price,
                Notes = notes,
                Status = SessionStatus.Pending,
                PaymentMethod = PaymentMethod.Online,
                PaymentStatus = PaymentStatus.Completed
            };

            _context.BookedSessions.Add(session);

            _context.Payments.Add(new Models.Payment
            {
                EntityType = PaymentEntityType.Session,
                EntityId = memberId,
                Amount = amountCents / 100m,
                TransactionId = Request.Query["payment_intent"],
                Status = PaymentStatus.Completed,
                PaidAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var (email, name) = await GetMemberContact(memberId);
            var trainerName = trainer != null ? $"{trainer.FirstName} {trainer.LastName}" : "";
            await _emailService.SendPaymentConfirmationAsync(email, name,
                "Sesiune rezervată — FitnessClub",
                EmailTemplate(name, $@"
<p>Sesiunea ta personală a fost rezervată cu succes!</p>
<p>Antrenor: <strong>{trainerName}</strong></p>
<p>Data: <strong>{date:dd MMMM yyyy}</strong> la <strong>{startTime:HH:mm}</strong></p>
<p>Durată: <strong>60 minute</strong></p>
<p>Suma achitată: <strong>{price} RON</strong></p>"));
        }

        private async Task HandleClass(IDictionary<string, string> meta, long amountCents)
        {
            var memberId = int.Parse(meta["member_id"]);
            var scheduleId = int.Parse(meta["schedule_id"]);
            var price = decimal.Parse(meta["price"]);

            var schedule = await _context.GroupClassSchedules
                .Include(s => s.Enrollments)
                .Include(s => s.GroupClass)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null) return;

            var enrollment = new GroupClassEnrollment
            {
                ScheduleId = scheduleId,
                MemberId = memberId,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Confirmed,
                PaymentMethod = PaymentMethod.Online,
                PaymentStatus = PaymentStatus.Completed,
                PricePaid = price
            };

            schedule.CurrentParticipants++;
            _context.GroupClassEnrollments.Add(enrollment);

            _context.Payments.Add(new Models.Payment
            {
                EntityType = PaymentEntityType.GroupClass,
                EntityId = scheduleId,
                Amount = amountCents / 100m,
                TransactionId = Request.Query["payment_intent"],
                Status = PaymentStatus.Completed,
                PaidAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var (email, name) = await GetMemberContact(memberId);
            var className = schedule.GroupClass?.Name ?? "Clasă de grup";
            await _emailService.SendPaymentConfirmationAsync(email, name,
                $"Înscriere clasă — FitnessClub",
                EmailTemplate(name, $@"
<p>Te-ai înscris cu succes la clasa <strong>{className}</strong>!</p>
<p>Data: <strong>{schedule.Date:dd MMMM yyyy}</strong> la <strong>{schedule.StartTime:HH:mm}</strong></p>
<p>Locație: <strong>{schedule.Location}</strong></p>
<p>Suma achitată: <strong>{price} RON</strong></p>"));
        }

        private async Task HandleEvent(IDictionary<string, string> meta, long amountCents)
        {
            var memberId = int.Parse(meta["member_id"]);
            var eventId = int.Parse(meta["event_id"]);
            var price = decimal.Parse(meta["price"]);

            var ev = await _context.Events.FindAsync(eventId);

            var enrollment = new EventEnrollment
            {
                EventId = eventId,
                MemberId = memberId,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Confirmed,
                PaymentMethod = PaymentMethod.Online,
                PaymentStatus = PaymentStatus.Completed,
                PricePaid = price
            };

            _context.EventEnrollments.Add(enrollment);

            _context.Payments.Add(new Models.Payment
            {
                EntityType = PaymentEntityType.Event,
                EntityId = eventId,
                Amount = amountCents / 100m,
                TransactionId = Request.Query["payment_intent"],
                Status = PaymentStatus.Completed,
                PaidAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var (email, name) = await GetMemberContact(memberId);
            var eventName = ev?.Name ?? "Eveniment";
            await _emailService.SendPaymentConfirmationAsync(email, name,
                $"Înscriere eveniment — FitnessClub",
                EmailTemplate(name, $@"
<p>Te-ai înscris cu succes la evenimentul <strong>{eventName}</strong>!</p>
<p>Suma achitată: <strong>{price} RON</strong></p>
<p>Ne vedem acolo!</p>"));
        }
    }
}
