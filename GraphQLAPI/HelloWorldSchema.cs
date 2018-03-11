using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLAPI
{
    public class HelloWorldSchema : Schema
    {
        public HelloWorldSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<IQuery>();
        }
    }
}
