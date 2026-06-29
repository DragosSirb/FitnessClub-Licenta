using FitnessClub.Data;
using FitnessClub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Shop
{
    public class ProductDetailModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductDetailModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Product? Product { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (Product == null)
                return RedirectToPage("/Shop/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int id, int quantity = 1)
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            if (!User.IsInRole("member"))
                return RedirectToPage(new { id });

            var user = await _userManager.GetUserAsync(User);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user!.Id);
            if (member == null) return RedirectToPage(new { id });

            var product = await _context.Products.FindAsync(id);
            if (product == null || !product.IsActive || product.Stock < quantity)
            {
                TempData["Error"] = "Produsul nu este disponibil.";
                return RedirectToPage(new { id });
            }

            var cart = GetCart();
            if (cart.ContainsKey(id))
                cart[id] += quantity;
            else
                cart[id] = quantity;

            SaveCart(cart);
            TempData["Success"] = "Adăugat în coș!";
            return RedirectToPage(new { id });
        }

        private Dictionary<int, int> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("cart") ?? "{}";
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson) ?? new();
        }

        private void SaveCart(Dictionary<int, int> cart)
        {
            HttpContext.Session.SetString("cart", System.Text.Json.JsonSerializer.Serialize(cart));
        }
    }
}
