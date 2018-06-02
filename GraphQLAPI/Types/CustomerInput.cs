using System;
using GraphQL.Types;

namespace GraphQLAPI.Types
{
	public class CustomerInput : InputObjectGraphType
    {
		public CustomerInput()
        {
			Name = "CustomerInput";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<StringGraphType>>("billingAddress");
        }
    }
}
