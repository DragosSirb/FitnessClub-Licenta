using FitnessClub.Data;
using FitnessClub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Shop
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<ProductCategory> Categories { get; set; } = new();
        public List<Product> Products { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.ProductCategories.ToListAsync();

            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == CategoryId.Value);

            Products = await query.OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity = 1, int? categoryId = null)
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            if (!User.IsInRole("member"))
                return RedirectToPage();

            var user = await _userManager.GetUserAsync(User);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user!.Id);
            if (member == null) return RedirectToPage();

            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive || product.Stock < quantity)
            {
                TempData["Error"] = "Produsul nu este disponibil.";
                return RedirectToPage();
            }

            var cart = GetCart();
            if (cart.ContainsKey(productId))
                cart[productId] += quantity;
            else
                cart[productId] = quantity;

            SaveCart(cart);
            return RedirectToPage(new { categoryId });
        }

        public IActionResult OnPostRemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.Remove(productId);
            SaveCart(cart);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            if (!User.IsInRole("member"))
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var user = await _userManager.GetUserAsync(User);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user!.Id);
            if (member == null) return RedirectToPage();

            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Coșul este gol.";
                return RedirectToPage();
            }

            return RedirectToPage("/Shop/Checkout");
        }

        public Dictionary<int, int> GetCart()
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
