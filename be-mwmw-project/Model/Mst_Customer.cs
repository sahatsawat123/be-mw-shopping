namespace Shopping.Models
{
    public partial class Mst_Customer
    {
        public Guid CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string Address { get; set; }

        public string TaxId { get; set; }

        public string PhoneNo { get; set; }

        public Mst_Customer()
        {
            if (CustomerName == null)
            {
                CustomerName = "";
            }
            if (Address == null)
            {
                Address = "";
            }
            if (TaxId == null)
            {
                TaxId = "";
            }
            if (PhoneNo == null)
            {
                PhoneNo = "";
            }
        }
    }
}