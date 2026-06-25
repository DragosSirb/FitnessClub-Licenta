using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Services
{
    public class PaymentService
    {
        private readonly AppDbContext _db;

        public PaymentService(AppDbContext db)
        {
            _db = db;
        }

        public async Task HandlePayment(string type, IDictionary<string, string> metadata)
        {
            switch (type)
            {
                case "shop": await ProcessShopOrder(metadata); break;
                case "subscription": await ProcessSubscription(metadata); break;
                case "session": await ProcessTrainerSession(metadata); break;
                case "class": await ProcessGroupClassEnrollment(metadata); break;
                case "event": await ProcessEventEnrollment(metadata); break;
            }
        }

        private async Task ProcessShopOrder(IDictionary<string, string> metadata)
        {
            var memberId = int.Parse(metadata["member_id"]);
            var cartJson = metadata["cart"];
            var cart = System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<int, int>>(cartJson) ?? new();

            var productIds = cart.Keys.ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var total = products.Sum(p => p.Price * cart[p.Id]);

            var order = new Order
            {
                MemberId = memberId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                PaymentMethod = PaymentMethod.Online,
                PaymentStatus = PaymentStatus.Completed,
                TotalAmount = total
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            foreach (var product in products)
            {
                _db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    Quantity = cart[product.Id],
                    PriceAtPurchase = product.Price
                });
                product.Stock -= cart[product.Id];
            }

            await _db.SaveChangesAsync();
            await AddNotification(memberId, "Order Placed",
                $"Your order #{order.Id} has been placed successfully. Total: {total} RON.",
                NotificationType.Order, "/Member/Orders");
        }

        private async Task ProcessSubscription(IDictionary<string, string> metadata)
        {
            var memberId = int.Parse(metadata["member_id"]);
            var planId = int.Parse(metadata["plan_id"]);

            var plan = await _db.SubscriptionPlans.FindAsync(planId);
            if (plan == null) return;

            var subscription = new MemberSubscription
            {
                MemberId = memberId,
                PlanId = planId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
                Status = SubscriptionStatus.Active,
                PaymentMethod = PaymentMethod.Online,
                PaymentStatus = PaymentStatus.Completed,
                PricePaid = plan.Price
            };

            _db.MemberSubscriptions.Add(subscription);
            await _db.SaveChangesAsync();
            await AddNotification(memberId, "Subscription Activated",
                $"Your {plan.Name} subscription has been activated successfully.",
                NotificationType.Subscription, "/Member/Dashboard");
        }

        private async Task ProcessTrainerSession(IDictionary<string, string> metadata)
        {
            var memberId = int.Parse(metadata["member_id"]);
            var trainerId = int.Parse(metadata["trainer_id"]);
            var date = DateTime.Parse(metadata["date"]);
            var startTime = TimeOnly.Parse(metadata["start_time"]);
            var price = decimal.Parse(metadata["price"]);

            var session = new BookedSession
            {
                MemberId = memberId,
                TrainerId = trainerId,
                Date = date,
                StartTime = startTime,
                PricePaid = price,
                Status = SessionStatus.Pending,
                PaymentMethod = PaymentMethod.Online,
                PaymentStatus = PaymentStatus.Completed
            };

            _db.BookedSessions.Add(session);
            await _db.SaveChangesAsync();
            await AddNotification(memberId, "Session Booked",
                $"Your training session has been booked for {date:dd MMM yyyy} at {startTime}.",
                NotificationType.Session, "/Member/BookSession?tab=my");
        }

        private async Task ProcessGroupClassEnrollment(IDictionary<string, string> metadata)
        {
            var memberId = int.Parse(metadata["member_id"]);
            var scheduleId = int.Parse(metadata["schedule_id"]);
            var price = decimal.Parse(metadata["price"]);

            var enrollment = new GroupClassEnrollment
            {
                MemberId = memberId,
                ScheduleId = scheduleId,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Confirmed,
                PaymentMethod = PaymentMethod.Online,
                PaymentStatus = PaymentStatus.Completed,
                PricePaid = price
            };

            _db.GroupClassEnrollments.Add(enrollment);
            await _db.SaveChangesAsync();
            await AddNotification(memberId, "Class Enrollment Confirmed",
                "You have been successfully enrolled in the class.",
                NotificationType.Class, "/Member/Classes");
        }

        private async Task ProcessEventEnrollment(IDictionary<string, string> metadata)
        {
            var memberId = int.Parse(metadata["member_id"]);
            var eventId = int.Parse(metadata["event_id"]);
            var price = decimal.Parse(metadata["price"]);

            var enrollment = new EventEnrollment
            {
                MemberId = memberId,
                EventId = eventId,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Confirmed,
                PaymentMethod = PaymentMethod.Online,
                PaymentStatus = PaymentStatus.Completed,
                PricePaid = price
            };

            _db.EventEnrollments.Add(enrollment);
            await _db.SaveChangesAsync();
            await AddNotification(memberId, "Event Registration Confirmed",
                "You have been successfully registered for the event.",
                NotificationType.Event, "/Member/Events");
        }

        private async Task AddNotification(int memberId, string title, string message,
            NotificationType type, string link)
        {
            var member = await _db.Members.FindAsync(memberId);
            if (member == null) return;

            _db.Notifications.Add(new Notification
            {
                UserId = member.UserId,
                Title = title,
                Message = message,
                Type = type,
                Link = link
            });

            await _db.SaveChangesAsync();
        }
    }
}
