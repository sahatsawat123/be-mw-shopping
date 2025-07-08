using System.Security.Cryptography.X509Certificates;

namespace Shopping.Models
{
    public partial class Mst_ProductDTO
    {
        public Guid Product_Id { get; private set; } = Guid.NewGuid();

        public string Product_Name { get; set; }

        public string Product_Detail { get; set; }

        public decimal Price_Per_Unit { get; set; }

        public Guid Category_Id { get; set; }

        public Mst_ProductDTO()
        {
            if (Product_Name == null)
            {
                Product_Name = "";
            }
            if (Product_Detail == null)
            {
                Product_Detail = "";
            }
        }
    }
}