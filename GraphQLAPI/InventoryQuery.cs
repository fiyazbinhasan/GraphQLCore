using System.Collections.Generic;
using System.Linq;
using GraphQL.DataLoader;
using GraphQL.Types;
using GraphQLAPI;
using GraphQLAPI.Models;
using GraphQLAPI.Store;
using GraphQLAPI.Types;

namespace GraphQLAPI
{
    public class InventoryQuery : ObjectGraphType
    {
        public InventoryQuery(IDataLoaderContextAccessor accessor, IDataStore dataStore)
        {
            Field<ListGraphType<ItemType>, IEnumerable<Item>>()
                .Name("Items")
                .ResolveAsync(ctx =>
                {
                    var loader = accessor.Context.GetOrAddLoader("GetAllItems", () => dataStore.GetItemsAsync());
                    return loader.LoadAsync();
                });

            Field<ItemType, Item>()
                .Name("Item")
                .Argument<NonNullGraphType<StringGraphType>>("barcode", "item barcode")
                .ResolveAsync(ctx =>
                {
                    var barcode = ctx.GetArgument<string>("barcode");
                    return dataStore.GetItemByBarcodeAsync(barcode);
                });

            Field<ListGraphType<OrderType>, IEnumerable<Order>>()
                .Name("Orders")
                .ResolveAsync(ctx =>
                {
                    return dataStore.GetOrdersAsync();
                });

            Field<ListGraphType<CustomerType>, IEnumerable<Customer>>()
                .Name("Customers")
                .ResolveAsync(ctx =>
                {
                    return dataStore.GetCustomersAsync();
                });

            Field<ListGraphType<OrderItemType>, IEnumerable<OrderItem>>()
                .Name("OrderItem")
                .ResolveAsync(ctx =>
                {
                    return dataStore.GetOrderItemAsync();
                });
        }
    }
}