using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLAPI
{
    public interface IQuery : IObjectGraphType
    {
        void ConfigureFields();
    }
}
