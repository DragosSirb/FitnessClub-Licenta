using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using EventEntity = FitnessClub.Models.Event;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Stripe;
using PaymentMethod = FitnessClub.Models.Enums.PaymentMethod;

namespace FitnessClub.Pages.Member
{
    [Authorize(Roles = "member")]
    public class EventDetailModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public EventDetailModel(AppDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public EventEntity? Event { get; set; }
        public EventEnrollment? MyEnrollment { get; set; }
        public Models.Member? CurrentMember { get; set; }
        public string? ClientSecret { get; set; }
        public string? PublishableKey { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            CurrentMember = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
            if (CurrentMember == null) return RedirectToPage("/Index");

            Event = await _context.Events
                .Include(e => e.Enrollments)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (Event == null) return RedirectToPage("/Member/Events");

            MyEnrollment = Event.Enrollments.FirstOrDefault(e => e.MemberId == CurrentMember.Id && e.Status == EnrollmentStatus.Confirmed);

            return Page();
        }

        public async Task<IActionResult> OnPostAttendAsync(int id, PaymentMethod paymentMethod)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
            if (member == null) return RedirectToPage("/Index");

            var ev = await _context.Events.Include(e => e.Enrollments).FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToPage(new { id });
            }

            if (ev.Enrollments.Count(e => e.Status == EnrollmentStatus.Confirmed) >= ev.MaxParticipants)
            {
                TempData["Error"] = "This event is full.";
                return RedirectToPage(new { id });
            }

            if (ev.Enrollments.Any(e => e.MemberId == member.Id && e.Status == EnrollmentStatus.Confirmed))
            {
                TempData["Error"] = "You are already enrolled in this event.";
                return RedirectToPage(new { id });
            }

            if (paymentMethod == PaymentMethod.Online)
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(ev.Price * 100),
                    Currency = "ron",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
                    Metadata = new Dictionary<string, string>
                    {
                        { "payment_type", "event" },
                        { "member_id", member.Id.ToString() },
                        { "event_id", id.ToString() },
                        { "price", ev.Price.ToString() }
                    }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);
                ClientSecret = intent.ClientSecret;
                PublishableKey = _configuration["Stripe:PublishableKey"];

                await OnGetAsync(id);
                return Page();
            }
            else
            {
                var enrollment = new EventEnrollment
                {
                    EventId = id,
                    MemberId = member.Id,
                    EnrolledAt = DateTime.UtcNow,
                    Status = EnrollmentStatus.Confirmed,
                    PaymentMethod = paymentMethod,
                    PaymentStatus = PaymentStatus.Pending,
                    PricePaid = ev.Price
                };

                _context.EventEnrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "You have been enrolled in the event!";
                return RedirectToPage(new { id });
            }
        }

        public async Task<IActionResult> OnPostCancelAsync(int id, int enrollmentId)
        {
            var enrollment = await _context.EventEnrollments.FindAsync(enrollmentId);
            if (enrollment != null)
            {
                enrollment.Status = EnrollmentStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Enrollment cancelled successfully.";
            }
            return RedirectToPage(new { id });
        }
    }
}
