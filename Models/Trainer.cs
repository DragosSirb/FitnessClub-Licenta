namespace FitnessClub.Models
{
    public class Trainer
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Expertise { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public bool IsActive { get; set; } = true;
        public decimal SessionPrice { get; set; } = 80;

        public ApplicationUser User { get; set; } = null!;
        public ICollection<BookedSession> BookedSessions { get; set; } = new List<BookedSession>();
        public ICollection<GroupClass> GroupClasses { get; set; } = new List<GroupClass>();
        public ICollection<TrainerAvailability> Availability { get; set; } = new List<TrainerAvailability>();
        public ICollection<TrainerUnavailability> Unavailabilities { get; set; } = new List<TrainerUnavailability>();
        public ICollection<TrainerNote> Notes { get; set; } = new List<TrainerNote>();
    }
}
