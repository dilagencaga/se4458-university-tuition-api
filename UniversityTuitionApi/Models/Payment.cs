namespace UniversityTuitionApi.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string StudentNo { get; set; } = null!;
        public string Term { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    }
}
