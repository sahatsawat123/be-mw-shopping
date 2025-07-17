namespace Shopping.Models
{
    public partial class Trn_Sell_OrderSearch
    {
        public Guid Sell_Id { get; set; }
        public string Sell_Number { get; set; }

        public DateTime Sell_Date { get; set; }

        public string Remark { get; set; }

        public decimal Total_Price { get; set; }
        public decimal Vat_Rate { get; set; }

        public Guid Customer_Id { get; set; }

        public string Customer_Name { get; set; }

        public string Address { get; set; }

        public string Tax_Id { get; set; }

        public string Phone_No { get; set; }

        public Trn_Sell_OrderSearch()
        {
            if (Sell_Number == null)
            {
                Sell_Number = "";
            }
            if (Customer_Name == null)
            {
                Customer_Name = "";
            }
            if (Remark == null)
            {
                Remark = "";
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