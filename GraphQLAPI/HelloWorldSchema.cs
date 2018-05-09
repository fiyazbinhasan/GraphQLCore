using System;
using GraphQL;
using GraphQL.Types;

namespace GraphQLAPI
{
    public class HelloWorldSchema : Schema
    {
		public HelloWorldSchema(IDependencyResolver resolver) : base(resolver)
        {
			Query = resolver.Resolve<HelloWorldQuery>();
        }
    }
}
