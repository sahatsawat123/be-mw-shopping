namespace Shopping.Models
{
    public class SellOrderRequestSearch
    {
        public Trn_Sell_OrderSearch SearchSellOrder { get; set; } = new();
        public List<SellOrderDetailsSearch> SellOrderDetailsSearch { get; set; } = new();
    }
}