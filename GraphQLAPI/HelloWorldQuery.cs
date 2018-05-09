using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using GraphQLAPI;
using GraphQLAPI.Store;
using GraphQLAPI.Types;

namespace GraphQLAPI
{
    public class HelloWorldQuery : ObjectGraphType
    {
        public HelloWorldQuery(IDataStore dataStore)
        {
            Field<StringGraphType>(
                name: "hello",
                resolve: context => "world"
            );

            Field<StringGraphType>(
                name: "howdy",
                resolve: context => "universe"
            );

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