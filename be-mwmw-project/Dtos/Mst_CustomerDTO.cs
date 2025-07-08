namespace Shopping.Models
{
    public partial class Mst_CustomerDTO
    {
        public Guid Customer_Id { get; private set; } = Guid.NewGuid();

        public string Customer_Name { get; set; }

        public string Address { get; set; }

        public string Tax_Id { get; set; }

        public string Phone_No { get; set; }

        public Mst_CustomerDTO()
        {
            if (Customer_Name == null)
            {
                Customer_Name = "";
            }
            if (Address == null)
            {
                Address = "";
            }
            if (Tax_Id == null)
            {
                Tax_Id = "";
            }
            if (Phone_No == null)
            {
                Phone_No = "";
            }
        }
    }
}