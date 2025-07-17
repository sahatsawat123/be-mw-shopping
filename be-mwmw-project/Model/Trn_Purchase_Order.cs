namespace Shopping.Models
{
    public partial class Trn_Purchase_Order
    {
        public Guid Purchase_Order_Id { get; set; }
        public DateTime Order_Date { get; set; }

        public string Purchase_Order_Number { get; set; }

        public decimal Total_Price { get; set; }

        public Guid Supplier_Id { get; set; }

        public decimal Vat { get; set; }
        public decimal Vat_Rate { get; set; }

        public string Remark { get; set; }
        public Trn_Purchase_Order()
        {
            if (Purchase_Order_Number == null)
            {
                Purchase_Order_Number = "";
            }
            if (Remark == null)
            {
                Remark = "";
            }
        }
    }
}