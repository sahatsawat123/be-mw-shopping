namespace Shopping.Models
{
    public partial class Mst_CategoryDTO
    {
        public Guid Category_Id { get; private set; } = Guid.NewGuid();

        public string Category_Name { get; set; }

        public Mst_CategoryDTO()
        {
            if (Category_Name == null)
            {
                Category_Name = "";
            }
        }
    }
}