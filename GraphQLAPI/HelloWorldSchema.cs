using System;
using GraphQL;
using GraphQL.Types;

namespace GraphQLAPI
{
    public class HelloWorldSchema : Schema
    {
		public HelloWorldSchema(HelloWorldQuery query)
        {
            Query = query;
        }
    }
}
