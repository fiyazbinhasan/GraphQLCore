using System;
using GraphQL.Types;
using GraphQLAPI.Models;
using GraphQLAPI.Store;

namespace GraphQLAPI.Types
{
	public class OrderType : ObjectGraphType<Order>
    {
		public OrderType(IDataStore dataStore)
        {
			Field(o => o.Tag);
			Field(o => o.CreatedAt);
			Field<CustomerType, Customer>()
				.Name("Customer")
				.ResolveAsync(ctx =>
				{
					return dataStore.GetCustomerByIdAsync(ctx.Source.CustomerId);
				});
        }
    }
}
