using System;
using GraphQL.Types;
using GraphQLAPI.Models;

namespace GraphQLAPI.Types
{
	public class OrderType : ObjectGraphType<Order>
    {
        public OrderType()
        {
			Field(o => o.Tag);
			Field(o => o.CreatedAt);
        }
    }
}
