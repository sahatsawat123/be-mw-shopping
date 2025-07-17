namespace Shopping.Models
{
    public partial class Trn_Purchase_OrderDTO
    {
        public Guid Purchase_Order_Id { get; private set; }
        public DateTime Order_Date { get; private set; } = DateTime.Now;

        public string Purchase_Order_Number { get; set; }

        public Guid Supplier_Id { get; set; }

        public decimal Vat_Rate { get; set; }

        public string Remark { get; set; }

        public Trn_Purchase_OrderDTO()
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