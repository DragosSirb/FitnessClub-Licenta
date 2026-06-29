namespace FitnessClub.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal? GoalWeight { get; set; }
        public bool IsActive { get; set; } = true;

        public ApplicationUser User { get; set; } = null!;
        public ICollection<MemberSubscription> Subscriptions { get; set; } = new List<MemberSubscription>();
        public ICollection<DayPass> DayPasses { get; set; } = new List<DayPass>();
        public ICollection<BookedSession> BookedSessions { get; set; } = new List<BookedSession>();
        public ICollection<GroupClassEnrollment> ClassEnrollments { get; set; } = new List<GroupClassEnrollment>();
        public ICollection<EventEnrollment> EventEnrollments { get; set; } = new List<EventEnrollment>();
        public ICollection<BodyMeasurement> BodyMeasurements { get; set; } = new List<BodyMeasurement>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<TrainerNote> TrainerNotes { get; set; } = new List<TrainerNote>();
    }
}
