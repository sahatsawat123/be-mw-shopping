namespace Shopping.Models
{
    public class PurchaseOrderDetailsSearch
    {
        public Guid Purchase_Order_Detail_Id { get; set; } 
        public Guid Purchase_Order_Id { get; set; }
        public Guid Product_Id { get; set; }
        public int Amount { get; set; }
        public decimal Price_Per_Unit { get; set; }
        public string? Product_Name { get; set; }
        public string? Product_Detail { get; set; }
    }
}