using GraphQL.Types;
public class HelloWorldQuery : ObjectGraphType
{
    public HelloWorldQuery()
    {
        Field<StringGraphType>(
            name: "hello",
            resolve: context => "world"
        );

        Field<StringGraphType>(
            name: "howdy",
            resolve: context => "universe"
        );
    }
}