using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe;
using PaymentMethod = FitnessClub.Models.Enums.PaymentMethod;

namespace FitnessClub.Pages.Member
{
    [Authorize(Roles = "member")]
    public class BuyPlanModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public BuyPlanModel(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public SubscriptionPlan? Plan { get; set; }

        public string? ClientSecret { get; set; }
        public string? PublishableKey { get; set; }

        [BindProperty]
        public PaymentMethod PaymentMethod { get; set; }

        public async Task<IActionResult> OnGetAsync(int planId)
        {
            Plan = await _context.SubscriptionPlans.FindAsync(planId);

            if (Plan == null)
                return RedirectToPage("/Member/Plans");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int planId)
        {
            PublishableKey = _configuration["Stripe:PublishableKey"];

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Index");

            var member = _context.Members.FirstOrDefault(m => m.UserId == user.Id);
            var plan = await _context.SubscriptionPlans.FindAsync(planId);

            if (member == null || plan == null)
                return RedirectToPage("/Member/Plans");

            if (PaymentMethod == PaymentMethod.Online)
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(plan.Price * 100),
                    Currency = "ron",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "member_id", member.Id.ToString() },
                        { "plan_id", plan.Id.ToString() }
                    }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                ClientSecret = intent.ClientSecret;
                Plan = plan;

                return Page();
            }

            var subscription = new MemberSubscription
            {
                MemberId = member.Id,
                PlanId = plan.Id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(plan.DurationDays),
                Status = SubscriptionStatus.Active,
                PaymentMethod = PaymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                PricePaid = plan.Price
            };

            _context.MemberSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Member/Dashboard");
        }
    }
}
