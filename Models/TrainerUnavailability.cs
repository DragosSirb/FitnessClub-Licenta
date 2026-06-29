namespace FitnessClub.Models
{
    public class TrainerUnavailability
    {
        public int Id { get; set; }
        public int TrainerId { get; set; }
        public DateTime Date { get; set; }
        public string? Reason { get; set; }

        public Trainer Trainer { get; set; } = null!;
    }
}
