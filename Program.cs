using FitnessClub.Data;
using FitnessClub.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using FitnessClub.Services;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<NotificationFilter>();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddFolderApplicationModelConvention("/", model =>
        model.Filters.Add(new Microsoft.AspNetCore.Mvc.ServiceFilterAttribute(typeof(NotificationFilter))));
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<StripeSettings>(
  builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/Login";
});
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<EmailService>();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

app.MapPost("/webhook", async (HttpRequest request, AppDbContext db) =>
{
    var json = await new StreamReader(request.Body).ReadToEndAsync();
    var webhookSecret = builder.Configuration["Stripe:WebhookSecret"];

    try
    {
        var stripeEvent = EventUtility.ConstructEvent(
            json,
            request.Headers["Stripe-Signature"],
            webhookSecret
        );

        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            var metadata = paymentIntent!.Metadata;

            var memberId = int.Parse(metadata["member_id"]);
            var cartJson = metadata["cart"];
            var cart = System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<int, int>>(cartJson) ?? new();

            var member = await db.Members.FindAsync(memberId);
            var productIds = cart.Keys.ToList();
            var products = await db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var total = products.Sum(p => p.Price * cart[p.Id]);

            var order = new Order
            {
                MemberId = memberId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                PaymentMethod = FitnessClub.Models.Enums.PaymentMethod.Online,
                PaymentStatus = PaymentStatus.Completed,
                TotalAmount = total
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            foreach (var product in products)
            {
                db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    Quantity = cart[product.Id],
                    PriceAtPurchase = product.Price
                });
                product.Stock -= cart[product.Id];
            }

            await db.SaveChangesAsync();

            var notification = new Notification
            {
              UserId = member!.UserId,
              Title = "Comandă plasată",
              Message = $"Comanda ta #{order.Id} a fost plasată cu succes. Total: {total} RON.",
              Type = NotificationType.Order,
              Link = "/Member/Orders"
            };
            db.Notifications.Add(notification);


            await db.SaveChangesAsync();
        }

        return Results.Ok();
    }
    catch (StripeException e)
    {
        return Results.BadRequest(e.Message);
    }
});

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();

public partial class Program { }
