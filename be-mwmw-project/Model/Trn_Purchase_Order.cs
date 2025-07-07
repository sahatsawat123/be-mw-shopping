namespace Shopping.Models
{
    public partial class Trn_Purchase_Order
    {
        public Guid PurchaseOrderId { get; set; }
        public DateTime OrderDate { get; set; }

        public string PurchaseOrderNumber { get; set; }

        public decimal TotalPrice { get; set; }

        public Guid SupplierId { get; set; }

        public decimal Vat { get; set; }
        public decimal VatRate { get; set; }

        public string Remark { get; set; }

        public Trn_Purchase_Order()
        {
            if (PurchaseOrderNumber == null)
            {
                PurchaseOrderNumber = "";
            }
            if (Remark == null)
            {
                Remark = "";
            }
        }
    }
}