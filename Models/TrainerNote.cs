namespace FitnessClub.Models
{
    public class TrainerNote
    {
        public int Id { get; set; }
        public int TrainerId { get; set; }
        public int MemberId { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Note { get; set; } = string.Empty;

        public Trainer Trainer { get; set; } = null!;
        public Member Member { get; set; } = null!;
    }
}
