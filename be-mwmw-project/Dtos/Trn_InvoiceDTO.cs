namespace Shopping.Models
{
    public partial class Trn_InvoiceDTO
    {
        public Guid SellId { get; private set;}
        public string InvoiceNumber { get; set; }

        public Trn_InvoiceDTO()
        { 
            if (InvoiceNumber == null)
            {
                InvoiceNumber = "";
            }
        }
    }
}