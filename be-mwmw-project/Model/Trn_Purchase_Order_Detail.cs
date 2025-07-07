namespace Shopping.Models
{
    public partial class Trn_Purchase_Order_Detail
    {
        public Guid PurchaseOrderDetailId { get; set; }
        public Guid PurchaseOrderId { get; set; }

        public Guid ProductId { get; set; }

        public int Amount { get; set; }

        public Guid SupplierId { get; set; }

        public decimal PricePerUnit { get; set; }
    }
}