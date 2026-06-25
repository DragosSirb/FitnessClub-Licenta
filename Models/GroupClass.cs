namespace FitnessClub.Models
{
    public class GroupClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TrainerId { get; set; }
        public int DurationMinutes { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IncludedInSubscription { get; set; } = true;

        public Trainer Trainer { get; set; } = null!;
        public ICollection<GroupClassSchedule> Schedules { get; set; } = new List<GroupClassSchedule>();
    }
}