using System;
using GraphQL.Types;
using GraphQLAPI.Models;

namespace GraphQLAPI.Types
{
	public class OrderItemScalerType : ObjectGraphType<OrderItem>
    {
		public OrderItemScalerType()
        {
			Field(i => i.Quantity);
			Field(i => i.ItemId);
			Field(i => i.OrderId);
        }
    }
}
