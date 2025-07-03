namespace Testing.Models
{
    public class Asset
    {
        public int Id { get; set; }

        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
    
    public class AssetPrice
    {
        public int Id { get; set; }

        public string Symbol { get; set; } = String.Empty;

        public decimal Price { get; set; }

        public DateTime Update { get; set; }
    }
}
