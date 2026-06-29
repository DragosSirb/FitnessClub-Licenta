namespace FitnessClub.Models
{
    public class Quote
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}