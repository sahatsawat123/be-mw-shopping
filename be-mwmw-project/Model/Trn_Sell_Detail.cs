namespace Shopping.Models
{
    public partial class Trn_Sell_Detail
    {
        public Guid SellDetailId { get; set; }
        public Guid SellId { get; set; }

        public Guid ProductId { get; set; }

        public int Amount { get; set; }

        public decimal PricePerUnit { get; set; }
    }
}