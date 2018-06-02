using GraphQL.Types;
using GraphQLAPI.Models;
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
			Field<ItemType, Item>()
				.Name("createItem")
				.Argument<NonNullGraphType<ItemInputType>>("item", "item input")
				.ResolveAsync(ctx =>
			    {
				    var item = ctx.GetArgument<Item>("item");
				    return dataStore.AddItemAsync(item);
			    });

			Field<OrderType, Order>()
				.Name("createOrder")
				.Argument<NonNullGraphType<OrderInputType>>("order", "order input")
				.ResolveAsync(ctx =>
				{
					var order = ctx.GetArgument<Order>("order");
					return dataStore.AddOrderAsync(order);
				});

			Field<CustomerType, Customer>()
                .Name("createCustomer")
				.Argument<NonNullGraphType<CustomerInput>>("customer", "customer input")
                .ResolveAsync(ctx =>
                {
				    var customer = ctx.GetArgument<Customer>("customer");
                    return dataStore.AddCustomerAsync(customer);
                });
        }
    }
}
