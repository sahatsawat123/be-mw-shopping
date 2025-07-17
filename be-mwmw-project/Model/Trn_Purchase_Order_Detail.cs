namespace Shopping.Models
{
    public partial class Trn_Purchase_Order_Detail
    {
        public Guid Purchase_Order_Detail_Id { get; set; }
        public Guid Purchase_Order_Id { get; set; }

        public Guid Product_Id { get; set; }

        public int Amount { get; set; }

        public decimal Price_Per_Unit { get; set; }
    }
}