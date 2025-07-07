namespace Shopping.Models
{
    public partial class Mst_Category
    {
        public Guid Category_Id { get; set; }

        public string Category_Name { get; set; }

        public Mst_Category()
        {
            if (Category_Name == null)
            {
                Category_Name = "";
            }
        }
    }
}