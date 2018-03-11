using GraphQL.Types;
using GraphQLAPI;

public class HelloWorldQuery : ObjectGraphType, IQuery
{
    public HelloWorldQuery()
    {
        ConfigureFields();
    }

    public void ConfigureFields()
    {
        Field<StringGraphType>(
                    name: "hello",
                    resolve: context => "world"
                );
    }
}

public class HelloUniverseQuery : ObjectGraphType, IQuery
{
    public HelloUniverseQuery()
    {
        ConfigureFields();
    }

    public void ConfigureFields()
    {
        Field<StringGraphType>(
            name: "howdy",
            resolve: context => "universe"
        );
    }
}