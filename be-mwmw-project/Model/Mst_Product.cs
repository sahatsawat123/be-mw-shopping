using System.Security.Cryptography.X509Certificates;

namespace Shopping.Models
{
    public partial class Mst_Product
    {
        public Guid ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductDetails { get; set; }

        public decimal PricePerUnit { get; set; }

        public Guid CategoryId { get; set; }

        public Mst_Product()
        {
            if (ProductName == null)
            {
                ProductName = "";
            }
            if (ProductDetails == null)
            {
                ProductDetails = "";
            }
        }
    }
}