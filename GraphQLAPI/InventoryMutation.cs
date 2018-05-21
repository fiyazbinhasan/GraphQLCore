using GraphQL.Types;
using GraphQLAPI.Store;
using GraphQLAPI.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLAPI
{
    public class InventoryMutation : ObjectGraphType
    {
        public InventoryMutation(IDataStore dataStore)
        {
            /*
            
            mutation ($item: ItemInput!) {
  createItem(item: $item) {
    barcode
    title
    sellingPrice
  }
}

             */

            Field<ItemType>(
                "createItem",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ItemInputType>> { Name = "item" }
                ),
                resolve: context =>
                {
                    var item = context.GetArgument<Item>("item");
                    return dataStore.AddItem(item);
                });
        }
    }
}
