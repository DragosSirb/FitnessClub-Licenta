using System.ComponentModel.DataAnnotations;
using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Admin.Plans
{
    [Authorize(Roles = "admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<SubscriptionPlan> Plans { get; set; } = new();

        [BindProperty]
        public PlanInput Input { get; set; } = new();

        public class PlanInput
        {
            [Required]
            public string Name { get; set; } = string.Empty;

            [Required, Range(0.01, 99999)]
            public decimal Price { get; set; }

            [Required, Range(1, 3650)]
            public int DurationDays { get; set; }

            public string Description { get; set; } = string.Empty;

            public bool IncludesGroupClasses { get; set; }

            [Required]
            public SubscriptionPlanType Type { get; set; }
        }

        public async Task OnGetAsync()
        {
            Plans = await _context.SubscriptionPlans.OrderBy(p => p.Price).ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            _context.SubscriptionPlans.Add(new SubscriptionPlan
            {
                Name = Input.Name,
                Price = Input.Price,
                DurationDays = Input.DurationDays,
                Description = Input.Description,
                IncludesGroupClasses = Input.IncludesGroupClasses,
                IsActive = true,
                Type = Input.Type
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Plan created.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditPlanAsync(int planId, string name, string description, decimal price, int durationDays, bool includesGroupClasses)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan != null)
            {
                plan.Name = name;
                plan.Description = description;
                plan.Price = price;
                plan.DurationDays = durationDays;
                plan.IncludesGroupClasses = includesGroupClasses;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Plan updated.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan != null)
            {
                plan.IsActive = !plan.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan != null)
            {
                _context.SubscriptionPlans.Remove(plan);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
