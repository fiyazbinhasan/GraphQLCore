using System;
namespace GraphQLAPI.Models
{
	public class Item
    {
        public int ItemId { get; set; }
        public string Barcode { get; set; }
        public string Title { get; set; }
        public decimal SellingPrice { get; set; }
    }
}