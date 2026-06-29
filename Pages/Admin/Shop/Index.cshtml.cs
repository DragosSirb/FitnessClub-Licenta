using System.ComponentModel.DataAnnotations;
using FitnessClub.Data;
using FitnessClub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Pages.Admin.Shop
{
    [Authorize(Roles = "admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public IndexModel(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public List<Product> Products { get; set; } = new();
        public List<ProductCategory> Categories { get; set; } = new();

        [BindProperty]
        public ProductInput Input { get; set; } = new();

        [BindProperty]
        public CategoryInput CatInput { get; set; } = new();

        public class ProductInput
        {
            [Required] public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            [Required] public int CategoryId { get; set; }
            [Required, Range(0.01, 99999)] public decimal Price { get; set; }
            [Required, Range(0, 99999)] public int Stock { get; set; }
            public IFormFile? Image { get; set; }
        }

        public class CategoryInput
        {
            [Required] public string Name { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
            Categories = await _context.ProductCategories.ToListAsync();
            Products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateProductAsync()
        {
            if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }

            string? imageUrl = null;
            if (Input.Image != null && Input.Image.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"product_{Guid.NewGuid()}{Path.GetExtension(Input.Image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await Input.Image.CopyToAsync(stream);
                imageUrl = $"/uploads/products/{fileName}";
            }

            _context.Products.Add(new Product
            {
                Name = Input.Name,
                Description = Input.Description,
                CategoryId = Input.CategoryId,
                Price = Input.Price,
                Stock = Input.Stock,
                ImageUrl = imageUrl,
                IsActive = true
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Produs creat.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStockAsync(int productId, int stock)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.Stock = stock;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditProductAsync(int productId, string name, string description, decimal price, int stock, int categoryId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.Name = name;
                product.Description = description;
                product.Price = price;
                product.Stock = stock;
                product.CategoryId = categoryId;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Produs actualizat.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateImageAsync(int productId, IFormFile? image)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null && image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"product_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);
                product.ImageUrl = $"/uploads/products/{fileName}";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Imagine actualizată.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null) { product.IsActive = !product.IsActive; await _context.SaveChangesAsync(); }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateCategoryAsync()
        {
            if (!string.IsNullOrEmpty(CatInput.Name))
            {
                _context.ProductCategories.Add(new ProductCategory { Name = CatInput.Name });
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
