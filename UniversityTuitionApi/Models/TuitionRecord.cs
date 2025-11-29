namespace UniversityTuitionApi.Models
{
    public class TuitionRecord
    {
        public int Id { get; set; }
        public string StudentNo { get; set; } = null!;
        public string Term { get; set; } = null!;
        public decimal TuitionTotal { get; set; }
        public decimal Balance { get; set; }
    }
}
