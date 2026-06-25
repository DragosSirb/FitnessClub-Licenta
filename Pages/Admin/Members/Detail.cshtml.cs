using System.ComponentModel.DataAnnotations;
using FitnessClub.Data;
using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MemberEntity = FitnessClub.Models.Member;

namespace FitnessClub.Pages.Admin.Members
{
    [Authorize(Roles = "admin")]
    public class DetailModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public MemberEntity Member { get; set; } = null!;

        [BindProperty]
        public EditInput Input { get; set; } = new();

        public class EditInput
        {
            [Required] public string FirstName { get; set; } = string.Empty;
            [Required] public string LastName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string Gender { get; set; } = string.Empty;
            public DateTime DateOfBirth { get; set; }
            public decimal? GoalWeight { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var member = await _context.Members
                .Include(m => m.User)
                .Include(m => m.Subscriptions).ThenInclude(s => s.Plan)
                .Include(m => m.BookedSessions).ThenInclude(s => s.Trainer)
                .Include(m => m.Orders).ThenInclude(o => o.Items).ThenInclude(i => i.Product)
                .Include(m => m.ClassEnrollments).ThenInclude(e => e.Schedule).ThenInclude(s => s.GroupClass)
                .Include(m => m.EventEnrollments).ThenInclude(e => e.Event)
                .Include(m => m.BodyMeasurements)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null) return RedirectToPage("Index");

            Member = member;
            Input = new EditInput
            {
                FirstName = member.FirstName,
                LastName = member.LastName,
                Phone = member.Phone,
                Address = member.Address,
                Gender = member.Gender,
                DateOfBirth = member.DateOfBirth,
                GoalWeight = member.GoalWeight
            };

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null) return RedirectToPage("Index");

            member.FirstName = Input.FirstName;
            member.LastName = Input.LastName;
            member.Phone = Input.Phone;
            member.Address = Input.Address;
            member.Gender = Input.Gender;
            member.DateOfBirth = Input.DateOfBirth;
            member.GoalWeight = Input.GoalWeight;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Member details saved.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                member.IsActive = !member.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { id });
        }
    }
}
