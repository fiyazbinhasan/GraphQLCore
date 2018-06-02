using System;
using System.Collections.Generic;
using GraphQL.Types;
using GraphQLAPI.Models;
using GraphQLAPI.Store;

namespace GraphQLAPI.Types
{
	public class CustomerType : ObjectGraphType<Customer>
    {
		public CustomerType(IDataStore dataStore)
        {
			Field(c => c.Name);         
			Field(c => c.BillingAddress);         
			Field<ListGraphType<OrderType>, IEnumerable<Order>>().Name("Orders").ResolveAsync(ctx => {
				return dataStore.GetOrdersByCustomerIdAsync(ctx.Source.CustomerId);
			});
        }
    }
}
