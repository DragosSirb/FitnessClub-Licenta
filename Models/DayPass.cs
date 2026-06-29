using FitnessClub.Models.Enums;

namespace FitnessClub.Models
{
    public class DayPass
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int PlanId { get; set; }
        public DateTime Date { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal PricePaid { get; set; }

        public Member Member { get; set; } = null!;
        public SubscriptionPlan Plan { get; set; } = null!;
    }
}
