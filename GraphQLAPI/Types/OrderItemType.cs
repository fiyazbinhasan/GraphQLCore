using System;
using GraphQL.Types;
using GraphQLAPI.Models;
using GraphQLAPI.Store;

namespace GraphQLAPI.Types
{
	public class OrderItemType : ObjectGraphType<OrderItem>
    {
		public OrderItemType(IDataStore dateStore)
		{   
            Field(i => i.ItemId);      

			Field<ItemType, Item>().Name("Item").ResolveAsync(ctx =>
            {
				return dateStore.GetItemByIdAsync(ctx.Source.ItemId);
            });         

			Field(i => i.Quantity);

			Field(i => i.OrderId);

            Field<OrderType, Order>().Name("Order").ResolveAsync(ctx =>
            {
				return dateStore.GetOrderByIdAsync(ctx.Source.OrderId);
            });

        }
    }
}
