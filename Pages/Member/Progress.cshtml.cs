using System.ComponentModel.DataAnnotations;
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
    public class ProgressModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly Random RandomGenerator = new();

        public ProgressModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Models.Member? CurrentMember { get; set; }
        public List<BodyMeasurement> Measurements { get; set; } = new();
        public Quote? RandomQuote { get; set; }
        public bool ShowForm { get; set; }

        [BindProperty] public int? EditMeasurementId { get; set; }
        [BindProperty] public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public DateTime Date { get; set; } = DateTime.Today;

            [Required]
            public decimal Weight { get; set; }

            public decimal? Height { get; set; }
            public decimal? BodyFatPercentage { get; set; }
            public decimal? ChestCm { get; set; }
            public decimal? WaistCm { get; set; }
            public decimal? HipsCm { get; set; }
            public string? Notes { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool log = false)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            CurrentMember = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
            if (CurrentMember == null) return RedirectToPage("/Index");

            ShowForm = log;
            await LoadData(CurrentMember.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            CurrentMember = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
            if (CurrentMember == null) return RedirectToPage("/Index");

            if (!ModelState.IsValid)
            {
                ShowForm = true;
                await LoadData(CurrentMember.Id);
                return Page();
            }

            if (EditMeasurementId.HasValue)
            {
                var m = await _context.BodyMeasurements
                    .FirstOrDefaultAsync(b => b.Id == EditMeasurementId && b.MemberId == CurrentMember.Id);
                if (m != null)
                {
                    m.Date = Input.Date;
                    m.Weight = Input.Weight;
                    m.Height = Input.Height;
                    m.BodyFatPercentage = Input.BodyFatPercentage;
                    m.ChestCm = Input.ChestCm;
                    m.WaistCm = Input.WaistCm;
                    m.HipsCm = Input.HipsCm;
                    m.Notes = Input.Notes;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Măsurătoare actualizată cu succes!";
                }
            }
            else
            {
                var measurement = new BodyMeasurement
                {
                    MemberId = CurrentMember.Id,
                    Date = Input.Date,
                    Weight = Input.Weight,
                    Height = Input.Height,
                    BodyFatPercentage = Input.BodyFatPercentage,
                    ChestCm = Input.ChestCm,
                    WaistCm = Input.WaistCm,
                    HipsCm = Input.HipsCm,
                    Notes = Input.Notes
                };
                _context.BodyMeasurements.Add(measurement);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Progres înregistrat cu succes!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user!.Id);
            if (member == null) return RedirectToPage();

            var m = await _context.BodyMeasurements
                .FirstOrDefaultAsync(b => b.Id == id && b.MemberId == member.Id);
            if (m != null)
            {
                _context.BodyMeasurements.Remove(m);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Măsurătoare ștearsă.";
            }

            return RedirectToPage();
        }

        private async Task LoadData(int memberId)
        {
            Measurements = await _context.BodyMeasurements
                .Where(b => b.MemberId == memberId)
                .OrderByDescending(b => b.Date)
                .ToListAsync();

            var quotes = await _context.Quotes.Where(q => q.IsActive).ToListAsync();
            if (quotes.Any())
                RandomQuote = quotes[RandomGenerator.Next(quotes.Count)];
        }
    }
}
