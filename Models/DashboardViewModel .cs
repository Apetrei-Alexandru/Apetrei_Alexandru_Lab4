namespace Apetrei_Alexandru_Lab4.Models
{
    public class DashboardViewModel
    {
        public int TotalPredictions { get; set; }
        public List<PaymentTypeStat> PaymentTypeStats { get; set; } = new();
        public List<PriceBucketStat> PriceBuckets { get; set; } = new();

        // Intervalul selectat
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
