using System;
using GraphQL.Types;
using GraphQLAPI.Models;

namespace GraphQLAPI.Types
{
	public class OrderInputType : InputObjectGraphType
    {
		public OrderInputType()
		{
			Name = "OrderInput";
			Field<NonNullGraphType<StringGraphType>>("tag");
			Field<NonNullGraphType<DateGraphType>>("createdAt");
			Field<NonNullGraphType<IntGraphType>>("customerId");
        }
    }
}
