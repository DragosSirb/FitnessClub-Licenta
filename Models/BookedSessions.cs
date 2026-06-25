using FitnessClub.Models.Enums;

namespace FitnessClub.Models
{
    public class BookedSession
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int TrainerId { get; set; }
        public DateTime Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public int DurationMinutes { get; set; } = 60;
        public decimal PricePaid { get; set; }
        public string? Notes { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }

        public Member Member { get; set; } = null!;
        public Trainer Trainer { get; set; } = null!;
    }
}