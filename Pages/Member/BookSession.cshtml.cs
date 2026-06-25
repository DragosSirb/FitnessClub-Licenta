using System.ComponentModel.DataAnnotations;
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
using TrainerEntity = FitnessClub.Models.Trainer;

namespace FitnessClub.Pages.Member
{
  [Authorize(Roles = "member")]
  public class BookSessionModel : PageModel
  {
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public string ClientSecret { get; set; }
    public decimal SessionPrice { get; set; }
    private readonly IConfiguration _configuration;
    public string PublishableKey { get; set; }

    public BookSessionModel(AppDbContext context, UserManager<ApplicationUser> userManager,
      IConfiguration configuration)
    {
      _context = context;
      _userManager = userManager;
      _configuration = configuration;
    }

    public int Step { get; set; } = 1;
    public List<TrainerEntity> Trainers { get; set; } = new();
    public TrainerEntity? SelectedTrainer { get; set; }
    public List<TimeOnly> AvailableSlots { get; set; } = new();
    public List<(DateTime Date, int SlotsLeft)> AvailableDates { get; set; } = new();
    public List<BookedSession> MySessions { get; set; } = new();
    public Models.Member? CurrentMember { get; set; }

    [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "book";
    [BindProperty(SupportsGet = true)] public int? TrainerId { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? SelectedDate { get; set; }

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
      [Required] public int TrainerId { get; set; }

      [Required] public DateTime Date { get; set; }

      [Required] public TimeOnly StartTime { get; set; }

      public string? Notes { get; set; }

      [Required] public PaymentMethod PaymentMethod { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

      CurrentMember = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);

      if ((Tab == "my" || Tab == "history") && CurrentMember != null)
      {
        MySessions = await _context.BookedSessions
          .Include(s => s.Trainer)
          .Where(s => s.MemberId == CurrentMember.Id && s.Status != SessionStatus.Cancelled)
          .OrderByDescending(s => s.Date)
          .ThenBy(s => s.StartTime)
          .ToListAsync();
        return Page();
      }

      Trainers = await _context.Trainers
        .Where(t => t.IsActive)
        .Include(t => t.Availability)
        .ToListAsync();

      if (TrainerId.HasValue)
      {
        Step = 2;
        SelectedTrainer = Trainers.FirstOrDefault(t => t.Id == TrainerId);
        if (SelectedTrainer != null)
          SessionPrice = SelectedTrainer.SessionPrice;

        if (SelectedDate.HasValue && SelectedTrainer != null)
        {
          Step = 3;
          await LoadAvailableSlots(SelectedTrainer.Id, SelectedDate.Value);
        }
        else if (SelectedTrainer != null)
        {
          await LoadAvailableDates(SelectedTrainer);
        }
      }

      return Page();
    }

    private async Task LoadAvailableDates(TrainerEntity trainer)
    {
      var from = DateTime.Today.AddDays(1);
      var to = DateTime.Today.AddDays(28);

      var booked = await _context.BookedSessions
        .Where(s => s.TrainerId == trainer.Id && s.Date >= from && s.Date <= to && s.Status != SessionStatus.Cancelled)
        .Select(s => s.Date.Date)
        .ToListAsync();

      var unavailable = await _context.TrainerUnavailabilities
        .Where(u => u.TrainerId == trainer.Id && u.Date >= from && u.Date <= to)
        .Select(u => u.Date.Date)
        .ToListAsync();

      for (var date = from; date <= to; date = date.AddDays(1))
      {
        if (unavailable.Contains(date.Date)) continue;

        var avail = trainer.Availability.FirstOrDefault(a => a.DayOfWeek == date.DayOfWeek);
        if (avail == null) continue;

        var totalSlots = 0;
        var slot = avail.StartTime;
        while (slot.AddMinutes(60) <= avail.EndTime)
        {
          totalSlots++;
          slot = slot.AddMinutes(60);
        }

        var bookedCount = booked.Count(d => d == date.Date);
        var left = totalSlots - bookedCount;
        if (left > 0)
          AvailableDates.Add((date, left));
      }
    }

    public async Task<IActionResult> OnPostCancelAsync(int sessionId)
    {
      var user = await _userManager.GetUserAsync(User);
      var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user!.Id);
      if (member == null) return RedirectToPage();

      var session = await _context.BookedSessions
        .FirstOrDefaultAsync(s => s.Id == sessionId && s.MemberId == member.Id);

      if (session != null && session.Date >= DateTime.Today)
      {
        session.Status = SessionStatus.Cancelled;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Session cancelled successfully.";
      }

      return RedirectToPage(new { tab = "my" });
    }

    public async Task<IActionResult> OnPostAsync()
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

      var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
      if (member == null) return RedirectToPage("/Index");

      if (!ModelState.IsValid)
      {
        await OnGetAsync();
        return Page();
      }

      var trainer = await _context.Trainers.FindAsync(Input.TrainerId);
      if (trainer == null) return Page();

      decimal price = trainer.SessionPrice;

      if (Input.PaymentMethod == PaymentMethod.Online)
      {
        var options = new PaymentIntentCreateOptions
        {
          Amount = (long)(price * 100),
          Currency = "ron",
          AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
          {
            Enabled = true
          },
          Metadata = new Dictionary<string, string>
          {
            { "payment_type", "session" },
            { "member_id", member.Id.ToString() },
            { "trainer_id", trainer.Id.ToString() },
            { "date", Input.Date.ToString("yyyy-MM-dd") },
            { "start_time", Input.StartTime.ToString("HH:mm") },
            { "notes", Input.Notes },
            { "price", price.ToString() },
          }
        };
        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);
        ClientSecret = intent.ClientSecret;
        PublishableKey = _configuration["Stripe:PublishableKey"];

        TrainerId = Input.TrainerId;
        SelectedDate = Input.Date;
        await OnGetAsync();

        return Page();
      }
      else
      {
        var session = new BookedSession
        {
          MemberId = member.Id,
          TrainerId = Input.TrainerId,
          Date = Input.Date,
          StartTime = Input.StartTime,
          DurationMinutes = 60,
          PricePaid = price,
          Notes = Input.Notes,
          Status = SessionStatus.Pending,
          PaymentMethod = Input.PaymentMethod,
          PaymentStatus = PaymentStatus.Pending
        };

        _context.BookedSessions.Add(session);
        await _context.SaveChangesAsync();

        var notification = new Notification
        {
          UserId = user.Id,
          Title = "Session Request Sent",
          Message =
            $"Your session with {trainer.FirstName} {trainer.LastName} on {Input.Date:dd MMM yyyy} at {Input.StartTime}" +
            $" has been submitted and is awaiting confirmation.",
          Type = NotificationType.Session,
          Link = "/Member/BookSession?tab=my"
        };
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Session booked! Waiting for trainer confirmation.";
        return RedirectToPage("/Member/Dashboard");
      }
    }

    private async Task LoadAvailableSlots(int trainerId, DateTime date)
    {
      var isUnavailable = await _context.TrainerUnavailabilities
        .AnyAsync(u => u.TrainerId == trainerId && u.Date.Date == date.Date);
      if (isUnavailable) return;

      var dayOfWeek = date.DayOfWeek;
      var availability = await _context.TrainerAvailabilities
        .Where(a => a.TrainerId == trainerId && a.DayOfWeek == dayOfWeek)
        .FirstOrDefaultAsync();

      if (availability == null) return;

      var bookedTimes = await _context.BookedSessions
        .Where(s => s.TrainerId == trainerId && s.Date == date && s.Status != SessionStatus.Cancelled)
        .Select(s => s.StartTime)
        .ToListAsync();

      var slot = availability.StartTime;
      while (slot.AddMinutes(60) <= availability.EndTime)
      {
        if (!bookedTimes.Contains(slot))
          AvailableSlots.Add(slot);
        slot = slot.AddMinutes(60);
      }
    }
  }
}
