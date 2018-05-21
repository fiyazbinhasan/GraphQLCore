using System;
using GraphQL;
using GraphQL.Types;

namespace GraphQLAPI
{
    public class InventorySchema : Schema
    {
		public InventorySchema(InventoryQuery query, InventoryMutation mutation)
        {
			Query = query;
            Mutation = mutation;
        }
    }
}
