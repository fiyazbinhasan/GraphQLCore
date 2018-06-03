using System;
namespace GraphQLAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
      
		public int ItemId { get; set; }
        public Item Item { get; set; }

        public int Quantity { get; set; }      

        public int OrderId { get; set; }
		public Order Order { get; set; }
    }
}
