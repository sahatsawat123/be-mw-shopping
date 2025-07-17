namespace Shopping.Models
{
    public partial class Trn_Purchase_OrderSearch
    {
        public Guid Purchase_Order_Id { get; set; }
        public DateTime Order_Date { get; set; }

        public string Purchase_Order_Number { get; set; }

        public decimal Total_Price { get; set; }

        public decimal Vat { get; set; }
        public decimal Vat_Rate { get; set; }

        public string Remark { get; set; }

        public Guid Supplier_Id { get; set; }
        public string Supplier_Name { get; set; }

        public string Address { get; set; }

        public string Phone_No { get; set; }

        public string Tax_Id { get; set; }
        public Trn_Purchase_OrderSearch()
        {
            if (Purchase_Order_Number == null)
            {
                Purchase_Order_Number = "";
            }
            if (Remark == null)
            {
                Remark = "";
            }
            if (Supplier_Name == null)
            {
                Supplier_Name = "";
            }
            if (Address == null)
            {
                Address = "";
            }
            if (Phone_No == null)
            {
                Phone_No = "";
            }
            if (Tax_Id == null)
            {
                Tax_Id = "";
            }
        }

    }
}