using System;
using GraphQL.Types;

namespace GraphQLAPI.Types
{
	public class CustomerInputType : InputObjectGraphType
    {
        public CustomerInputType()
        {
            Name = "CustomerInputType";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<StringGraphType>>("billingAddress");
        }
    }
}
