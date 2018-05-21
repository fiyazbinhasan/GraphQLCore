using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using GraphQLAPI;
using GraphQLAPI.Store;
using GraphQLAPI.Types;

namespace GraphQLAPI
{
    public class InventoryQuery : ObjectGraphType
    {
        public InventoryQuery(IDataStore dataStore)
        {
            Field<ListGraphType<ItemType>>(
                "items",
                resolve: context =>
                {
                    return dataStore.GetItems();
                }
            );

            Field<ItemType>(
                "item",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "barcode" }),
                resolve: context =>
                {
                    var barcode = context.GetArgument<string>("barcode");
                    return dataStore.GetItemByBarcode(barcode);
                }
            );
        }
    }
}