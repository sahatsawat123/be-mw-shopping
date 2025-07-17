namespace Shopping.Models
{
    public partial class PurchaseOrderCreateRequestDTO
    {
        public Trn_Purchase_OrderDTO PurchaseOrder { get; set; } = new();
        public List<Trn_Purchase_Order_DetailDTO> OrderDetails { get; set; } = new();

        public PurchaseOrderCreateRequestDTO()
        {
        OrderDetails = new List<Trn_Purchase_Order_DetailDTO>();
        }
    }
}