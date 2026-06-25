using FitnessClub.Models.Enums;

namespace FitnessClub.Models
{
    public class GroupClassSchedule
    {
        public int Id { get; set; }
        public int GroupClassId { get; set; }
        public DateTime Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; } = 0;
        public ScheduleStatus Status { get; set; } = ScheduleStatus.Scheduled;

        public GroupClass GroupClass { get; set; } = null!;
        public ICollection<GroupClassEnrollment> Enrollments { get; set; } = new List<GroupClassEnrollment>();
    }
}
