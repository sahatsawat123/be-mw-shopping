namespace Shopping.Models
{
    public partial class SellOrderRequestDTO
    {
        public Trn_SellDTO SellOrder { get; set; } = new();
        public List<Trn_Sell_DetailDTO> OrderDetails { get; set; } = new();

        public bool? GenerateInvoicePdf { get; set; } = false;
        public bool? generateSellOrderPdf { get; private set; } = true;
    }
}