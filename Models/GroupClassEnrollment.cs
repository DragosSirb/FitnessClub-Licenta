using FitnessClub.Models.Enums;

namespace FitnessClub.Models
{
    public class GroupClassEnrollment
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public int MemberId { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Confirmed;
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal PricePaid { get; set; }

        public GroupClassSchedule Schedule { get; set; } = null!;
        public Member Member { get; set; } = null!;
    }
}