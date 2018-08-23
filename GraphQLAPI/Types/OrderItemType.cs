using System;
using GraphQL.DataLoader;
using GraphQL.Types;
using GraphQLAPI.Models;
using GraphQLAPI.Store;

namespace GraphQLAPI.Types
{
	public class OrderItemType : ObjectGraphType<OrderItem>
    {
		public OrderItemType(IDataStore dataStore, IDataLoaderContextAccessor accessor)
		{   
            Field(i => i.ItemId);      

			Field<ItemType, Item>()
				.Name("Item")
				.ResolveAsync(ctx =>
			    {
				    var itemsLoader = accessor.Context.GetOrAddBatchLoader<int, Item>("GetItemsById", fetchFunc: dataStore.GetItemsByIdAsync);
				    return itemsLoader.LoadAsync(ctx.Source.ItemId);  
                });         

			Field(i => i.Quantity);

			Field(i => i.OrderId);

            Field<OrderType, Order>()
				.Name("Order")
				.ResolveAsync(ctx =>
                {
				    var ordersLoader = accessor.Context.GetOrAddBatchLoader<int, Order>("GetOrdersById", fetchFunc: dataStore.GetOrdersByIdAsync);
				    return ordersLoader.LoadAsync(ctx.Source.OrderId);
                });

        }
    }
}
