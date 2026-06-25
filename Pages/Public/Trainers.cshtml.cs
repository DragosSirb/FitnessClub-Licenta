using FitnessClub.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrainerEntity = FitnessClub.Models.Trainer;

namespace FitnessClub.Pages.Public
{
    public class TrainersModel : PageModel
    {
        private readonly AppDbContext _context;

        public TrainersModel(AppDbContext context)
        {
            _context = context;
        }

        public List<TrainerEntity> Trainers { get; set; } = new();

        public async Task OnGetAsync()
        {
            Trainers = await _context.Trainers
                .Where(t => t.IsActive)
                .OrderBy(t => t.LastName)
                .ToListAsync();
        }
    }
}
