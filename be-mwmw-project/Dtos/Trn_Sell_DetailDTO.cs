namespace Shopping.Models
{
    public partial class Trn_Sell_DetailDTO
    {
        public Guid Sell_Detail_Id { get; private set; } = Guid.NewGuid();
        public Guid Sell_Id { get; private set; }

        public Guid Product_Id { get; set; }

        public int Amount { get; set; }
    }
}