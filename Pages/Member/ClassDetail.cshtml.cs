using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
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
    public class ClassDetailModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public ClassDetailModel(AppDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public GroupClassSchedule? Schedule { get; set; }
        public GroupClassEnrollment? MyEnrollment { get; set; }
        public Models.Member? CurrentMember { get; set; }
        public MemberSubscription? ActiveSubscription { get; set; }
        public string? ClientSecret { get; set; }
        public string? PublishableKey { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            CurrentMember = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
            if (CurrentMember == null) return RedirectToPage("/Index");

            Schedule = await _context.GroupClassSchedules
                .Include(s => s.GroupClass)
                    .ThenInclude(g => g.Trainer)
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (Schedule == null) return RedirectToPage("/Member/Classes");

            MyEnrollment = Schedule.Enrollments.FirstOrDefault(e => e.MemberId == CurrentMember.Id && e.Status == EnrollmentStatus.Confirmed);

            ActiveSubscription = await _context.MemberSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.MemberId == CurrentMember.Id && s.Status == SubscriptionStatus.Active)
                .FirstOrDefaultAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAttendAsync(int id, string paymentMethod)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
            if (member == null) return RedirectToPage("/Index");

            var schedule = await _context.GroupClassSchedules
                .Include(s => s.GroupClass)
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null || schedule.CurrentParticipants >= schedule.MaxParticipants)
            {
                TempData["Error"] = "Clasa este plină sau indisponibilă.";
                return RedirectToPage(new { id });
            }

            if (schedule.Enrollments.Any(e => e.MemberId == member.Id && e.Status == EnrollmentStatus.Confirmed))
            {
                TempData["Error"] = "Ești deja înscris la această clasă.";
                return RedirectToPage(new { id });
            }

            var activeSubscription = await _context.MemberSubscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.MemberId == member.Id && s.Status == SubscriptionStatus.Active);

            bool freeViaSubscription = activeSubscription?.Plan?.IncludesGroupClasses == true
                                       && schedule.GroupClass.IncludedInSubscription;

            if (freeViaSubscription)
            {
                var enrollment = new GroupClassEnrollment
                {
                    ScheduleId = id,
                    MemberId = member.Id,
                    EnrolledAt = DateTime.UtcNow,
                    Status = EnrollmentStatus.Confirmed,
                    PaymentMethod = PaymentMethod.Online,
                    PaymentStatus = PaymentStatus.Completed,
                    PricePaid = 0
                };
                schedule.CurrentParticipants++;
                _context.GroupClassEnrollments.Add(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ai fost înscris gratuit prin abonamentul tău!";
                return RedirectToPage(new { id });
            }
            else if (paymentMethod == "Online")
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = 5000,
                    Currency = "ron",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
                    Metadata = new Dictionary<string, string>
                    {
                        { "payment_type", "class" },
                        { "member_id", member.Id.ToString() },
                        { "schedule_id", id.ToString() },
                        { "price", "50" }
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
                var enrollment = new GroupClassEnrollment
                {
                    ScheduleId = id,
                    MemberId = member.Id,
                    EnrolledAt = DateTime.UtcNow,
                    Status = EnrollmentStatus.Confirmed,
                    PaymentMethod = PaymentMethod.InPerson,
                    PaymentStatus = PaymentStatus.Pending,
                    PricePaid = 50
                };
                schedule.CurrentParticipants++;
                _context.GroupClassEnrollments.Add(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ai fost înscris! Te rugăm să achiti la sală.";
                return RedirectToPage(new { id });
            }
        }

        public async Task<IActionResult> OnPostCancelAsync(int id, int enrollmentId)
        {
            var enrollment = await _context.GroupClassEnrollments
                .Include(e => e.Schedule)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment != null && enrollment.Status == EnrollmentStatus.Confirmed)
            {
                enrollment.Status = EnrollmentStatus.Cancelled;
                enrollment.Schedule.CurrentParticipants--;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Înscrierea a fost anulată cu succes.";
            }
            return RedirectToPage(new { id });
        }
    }
}
