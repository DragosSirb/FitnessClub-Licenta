namespace FitnessClub.Models
{
    public class BodyMeasurement
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public DateTime Date { get; set; }
        public decimal Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? BodyFatPercentage { get; set; }
        public decimal? ChestCm { get; set; }
        public decimal? WaistCm { get; set; }
        public decimal? HipsCm { get; set; }
        public string? Notes { get; set; }

        public Member Member { get; set; } = null!;
    }
}
