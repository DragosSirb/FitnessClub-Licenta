using FitnessClub.Data;
using FitnessClub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Member
{
    [Authorize(Roles = "member")]
    public class OrdersModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Order> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "active";

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
            if (member == null) return RedirectToPage("/Index");

            Orders = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.MemberId == member.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Page();
        }
    }
}
