using FitnessClub.Models.Enums;

namespace FitnessClub.Models
{
    public class SubscriptionPlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IncludesGroupClasses { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public SubscriptionPlanType Type { get; set; }

        public ICollection<MemberSubscription> MemberSubscriptions { get; set; } = new List<MemberSubscription>();
    }
}