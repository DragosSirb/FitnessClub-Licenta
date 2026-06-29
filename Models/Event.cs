using FitnessClub.Models.Enums;

namespace FitnessClub.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxParticipants { get; set; }
        public bool IsActive { get; set; } = true;
        public EventStatus Status { get; set; } = EventStatus.Upcoming;

        public ICollection<EventEnrollment> Enrollments { get; set; } = new List<EventEnrollment>();
    }
}