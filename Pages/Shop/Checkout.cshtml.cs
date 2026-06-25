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
using Product = FitnessClub.Models.Product;

namespace FitnessClub.Pages.Shop
{
  [Authorize(Roles = "member")]
  public class CheckoutModel : PageModel
  {
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public string ClientSecret { get; set; }
    private readonly IConfiguration _configuration;
    public string PublishableKey { get; set; }

    public CheckoutModel(AppDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
      _context = context;
      _userManager = userManager;
      _configuration = configuration;
    }

    public List<(Product Product, int Quantity)> CartItems { get; set; } = new();
    public decimal Total { get; set; }

    [BindProperty] public PaymentMethod PaymentMethod { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
      await LoadCart();
      if (!CartItems.Any())
        return RedirectToPage("/Shop/Index");

      return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
      PublishableKey = _configuration["Stripe:PublishableKey"];
      await LoadCart();
      if (!CartItems.Any())
        return RedirectToPage("/Shop/Index");

      var user = await _userManager.GetUserAsync(User);
      var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user!.Id);
      if (member == null) return RedirectToPage("/Index");

      if (PaymentMethod == PaymentMethod.Online)
      {
        var cartJson = HttpContext.Session.GetString("cart") ?? "{}";
        var options = new PaymentIntentCreateOptions
        {
          Amount = (long)(Total * 100),
          Currency = "ron",
          AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
          {
            Enabled = true
          },
          Metadata = new Dictionary<string, string>
          {
            { "member_id", member.Id.ToString() },
            { "cart", cartJson }
          }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        ClientSecret = intent.ClientSecret;
        return Page();
      }
      else
      {
        var order = new Order
        {
          MemberId = member.Id,
          CreatedAt = DateTime.UtcNow,
          Status = OrderStatus.Pending,
          PaymentMethod = PaymentMethod,
          PaymentStatus = PaymentStatus.Pending,
          TotalAmount = Total
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var (product, qty) in CartItems)
        {
          _context.OrderItems.Add(new OrderItem
          {
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = qty,
            PriceAtPurchase = product.Price
          });
          product.Stock -= qty;
        }

        await _context.SaveChangesAsync();
        HttpContext.Session.Remove("cart");

        var notification = new Notification
        {
          UserId = user!.Id,
          Title = "Order Placed",
          Message = $"Your order #{order.Id} has been placed successfully. Total: {Total} RON.",
          Type = NotificationType.Order,
          Link = "/Member/Orders"
        };
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Order #{order.Id} placed successfully!";
        return RedirectToPage("/Member/Orders");
      }
    }


    public IActionResult OnPostIncreaseQty(int productId)
    {
      var cart = GetCart();
      if (cart.ContainsKey(productId))
        cart[productId]++;
      SaveCart(cart);
      return RedirectToPage();
    }

    public IActionResult OnPostDecreaseQty(int productId)
    {
      var cart = GetCart();
      if (cart.ContainsKey(productId))
      {
        cart[productId]--;
        if (cart[productId] <= 0)
          cart.Remove(productId);
      }
      SaveCart(cart);
      if (!cart.Any())
        return RedirectToPage("/Shop/Index");
      return RedirectToPage();
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

    private async Task LoadCart()
    {
      var cartJson = HttpContext.Session.GetString("cart") ?? "{}";
      var cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson) ?? new();

      if (!cart.Any()) return;

      var productIds = cart.Keys.ToList();
      var products = await _context.Products
        .Where(p => productIds.Contains(p.Id))
        .ToListAsync();

      foreach (var product in products)
      {
        if (cart.TryGetValue(product.Id, out var qty))
        {
          CartItems.Add((product, qty));
          Total += product.Price * qty;
        }
      }
    }
  }
}
