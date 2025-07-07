namespace Shopping.Models
{
    public partial class Trn_Invoice
    {
        public Guid SellId { get; set; }
        public string InvoiceNumber { get; set; }

        public Trn_Invoice()
        { 
            if (InvoiceNumber == null)
            {
                InvoiceNumber = "";
            }
        }
    }
}