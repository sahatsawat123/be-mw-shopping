namespace Shopping.Models
{
    public class SellOrderDetailsSearch
    {
        public Guid Sell_Detail_Id { get; set; } 
        public Guid Sell_Id { get; set; }
        public Guid Product_Id { get; set; }
        public int Amount { get; set; }
        public decimal Price_Per_Unit { get; set; }
        public string? Product_Name { get; set; }
        public string? Product_Detail { get; set; }
    }
}