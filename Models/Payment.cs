using FitnessClub.Models.Enums;

namespace FitnessClub.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public PaymentEntityType EntityType { get; set; }
        public int EntityId { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionId { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime? PaidAt { get; set; }
    }
}
