namespace Apetrei_Alexandru_Lab4.Models
{
    public class PriceBucketStat
    {
        public string Label { get; set; } = string.Empty; // ex. "0-10", "10-20"
        public int Count { get; set; }
    }
}
