namespace Shopping.Models
{
    public partial class Mst_Supplier
    {
        public Guid SupplierId { get; set; }

        public string SupplierName { get; set; }

        public string Address { get; set; }

        public string PhoneNo { get; set; }

        public string TaxId { get; set; }

        public Mst_Supplier()
        { 
            if (SupplierName == null)
            {
                SupplierName = "";
            }
            if (Address == null)
            {
                Address = "";
            }
            if (PhoneNo == null)
            {
                PhoneNo = "";
            }
            if (TaxId == null)
            {
                TaxId = "";
            }
        }
    }
}