using GraphQL.Types;
using GraphQLAPI.Models;

namespace GraphQLAPI.Types
{
    public class ItemType : ObjectGraphType<Item>
    {
        public ItemType()
        {
            Field(i => i.Barcode);         
            Field(i => i.Title);         
            Field(i => i.SellingPrice);
        }
    }
}