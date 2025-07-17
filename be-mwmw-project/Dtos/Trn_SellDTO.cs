namespace Shopping.Models
{
    public partial class Trn_SellDTO
    {
        public Guid Sell_Id { get; private set;}
        public string Sell_Number { get; set; }

        public DateTime Sell_Date { get; private set; } = DateTime.Now;

        public Guid Customer_Id { get; set; }

        public string Remark { get; set; }
        public decimal Vat_Rate { get; set; }

        public Trn_SellDTO()
        {
            if (Sell_Number == null)
            {
                Sell_Number = "";
            }
            if (Remark == null)
            {
                Remark = "";
            }
        }
    }
}