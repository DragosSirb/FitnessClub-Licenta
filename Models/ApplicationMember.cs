using Microsoft.AspNetCore.Identity;

namespace FitnessClub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool ProfileCompleted { get; set; } = false;

        public Member? Member { get; set; }
        public Trainer? Trainer { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
