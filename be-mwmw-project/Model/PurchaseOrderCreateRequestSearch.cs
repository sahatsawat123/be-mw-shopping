namespace Shopping.Models
{
    public class PurchaseOrderCreateRequestSearch
    {
        public Trn_Purchase_OrderSearch PurchaseOrderSearch { get; set; } = new();
        public List<PurchaseOrderDetailsSearch> OrderDetailsSearch { get; set; } = new();
    }
}