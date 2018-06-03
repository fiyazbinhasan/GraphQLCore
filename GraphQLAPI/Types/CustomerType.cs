using System;
using System.Collections.Generic;
using GraphQL.DataLoader;
using GraphQL.Types;
using GraphQLAPI.Models;
using GraphQLAPI.Store;

namespace GraphQLAPI.Types
{
	public class CustomerType : ObjectGraphType<Customer>
    {
		public CustomerType(IDataStore dataStore, IDataLoaderContextAccessor accessor)
        {
			Field(c => c.Name);         
			Field(c => c.BillingAddress);         
			Field<ListGraphType<OrderType>, IEnumerable<Order>>()
				.Name("Orders")
				.ResolveAsync(ctx => 
			    {
				    var ordersLoader = accessor.Context.GetOrAddCollectionBatchLoader<int, Order>("GetOrdersByCustomerId", dataStore.GetOrdersByCustomerIdAsync);
				    return ordersLoader.LoadAsync(ctx.Source.CustomerId);
			    });
        }
    }
}
