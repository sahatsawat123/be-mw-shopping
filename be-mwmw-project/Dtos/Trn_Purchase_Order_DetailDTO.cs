namespace Shopping.Models
{
    public partial class Trn_Purchase_Order_DetailDTO
    {
        public Guid Purchase_Order_Detail_Id { get; private set; } = Guid.NewGuid();

        public Guid Purchase_Order_Id { get; private set; }

        public Guid Product_Id { get; set; }

        public int Amount { get; set; }

    }
}