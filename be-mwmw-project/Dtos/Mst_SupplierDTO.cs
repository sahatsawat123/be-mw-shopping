namespace Shopping.Models
{
    public partial class Mst_SupplierDTO
    {
        public Guid Supplier_Id { get; private set; } = Guid.NewGuid();

        public string Supplier_Name { get; set; }

        public string Address { get; set; }

        public string Phone_No { get; set; }

        public string Tax_Id { get; set; }

        public Mst_SupplierDTO()
        { 
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