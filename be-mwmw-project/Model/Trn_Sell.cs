namespace Shopping.Models
{
    public partial class Trn_Sell
    {
        public Guid SellId { get; set; }
        public string SellNumber { get; set; }

        public DateTime SellDate { get; set; }

        public Guid CustomerId { get; set; }

        public string Remark { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal VatRate { get; set; }

        public Trn_Sell()
        {
            if (SellNumber == null)
            {
                SellNumber = "";
            }
            if (Remark == null)
            {
                Remark = "";
            }
        }
    }
}