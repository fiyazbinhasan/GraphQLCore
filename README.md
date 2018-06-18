# GraphQL with ASP.NET Core (Part- I : Hello World)

Tired of REST? Let's talk about `GraphQL`. GraphQL provides a ***declarative***<sup>[[1]](#declarative)</sup> way in which you can fetch data from the server. You can [read](http://graphql.org/learn/) about every bit of goodness that is baked into GraphQL in the official site. However, in this series of blog posts, I'm going to deal with ASP.NET Core and will show how you can integrate `GraphQL` with it and use it as a query language for your API. 

<blockquote id="declarative">
<p> Meaning that you only declare the properties you need (In contrast to restful API where you call a specific endpoint for a fixed set of data and then you dig out the properties that you are actually looking for)</p> 
</blockquote>

To work with C#, the community has provided an open source port for GraphQL called [graphql-dotnet](https://github.com/graphql-dotnet/graphql-dotnet) and we are going to use that. So, let's get started, shall we?    

Start by creating a blank ASP.Net Core App.

    dotnet new web 

We are going to build the GraphQL middleware later (Next Post). But first, let's get our basics right. Assuming you already know a bit about GraphQL. So, consider a simple hello world app: we will query for a ***'hello'*** property to the server and it will return a ***'world'*** string. So, the property **hello** will definitely be a string type. 

In the blank app, add the [GraphQL package for dotnet](https://www.nuget.org/packages/GraphQL/2.0.0-alpha-863) and restore the package, 

    dotnet add package GraphQL --version 2.0.0-alpha-923

    dotnet restore

> This series of articles will be updated with each version updates of the package hence using the most recent alpha release. If I forget to update the article and the example code doesn't work, leave a comment below saying, `Update Needed`.

Let's create a `query` and call it `HelloWorldQuery`. It's a simple class used to define fields of the query. Any query class should be extended from the `ObjectGraphType`. So, the `HelloWorldQuery` class with a string `Field` should be as followings,

    using GraphQL.Types;
    public class HelloWorldQuery : ObjectGraphType
    {
        public HelloWorldQuery()
        {
            Field<StringGraphType>(
                name: "hello",
                resolve: context => "world"
            );
        }
    }

> Fields are defined in the constructor function of the query class.

Notice that we are using the generic version of the `Field` and passing a GraphQL string type (`StringGraphType`) to let the query know that the `hello` field contains a string type result.

Now that we have a query, next we need to build a schema out of it. 

In `Configure` method of `Startup.cs`, delete the existing code and paste the following code,

    var schema = new Schema { Query = new HelloWorldQuery() };

    app.Run(async (context) =>
    {
        var result = await new DocumentExecuter().ExecuteAsync(doc =>
        {
            doc.Schema = schema;
            doc.Query = @"
                query {
                    hello
                }
            ";
        }).ConfigureAwait(false);

        var json = new DocumentWriter(indent: true).Write(result)
        await context.Response.WriteAsync(json);
    });


Notice how we create a new GraphQL schema by assigning the `Schema` property with an instance of `HelloWorldQuery` object. 

[ExecuteAsync()](https://github.com/graphql-dotnet/graphql-dotnet/blob/514fa76063c05cf3e4d60514c1b6eedb5ac69722/src/GraphQL/Execution/DocumentExecuter.cs#L76) of the `DocumentExecuter` takes an Action of type [ExecutionOptions](https://github.com/graphql-dotnet/graphql-dotnet/blob/575bdf98a73b1737bb71455144d28aeed8bb6e24/src/GraphQL/Execution/ExecutionOptions.cs). There it initializes the schema and executes the provided query against it.

Carefully look at the string assigned to the `Query` (doc.Query). Here we are only asking for the `hello` field. You can rewrite the string as `{ hello }` (without the *query* part) and it will also work.

Finally, the result of the execution is converted to JSON using the [Wrtie()](https://github.com/graphql-dotnet/graphql-dotnet/blob/24157acb818cd0c1ff4012b8e311a9efa8fc53ae/src/GraphQL/Http/DocumentWriter.cs#L38) function of the `DocumentWriter` class. Last but not least we print out JSON to the response.

Now, run the application,

    dotnet run

You will get the following response in the  browser,

<a href="https://1.bp.blogspot.com/-VF4F4IwQ89c/WqPV5JkYQ7I/AAAAAAAAB0Y/w96OG5Ti4sE9MXsS8NOuakgL2aFppCz0wCLcBGAs/s1600/screencapture-localhost-5000-2018-03-10-18_53_29.png" imageanchor="1" ><img border="0" src="https://1.bp.blogspot.com/-VF4F4IwQ89c/WqPV5JkYQ7I/AAAAAAAAB0Y/w96OG5Ti4sE9MXsS8NOuakgL2aFppCz0wCLcBGAs/s1600/screencapture-localhost-5000-2018-03-10-18_53_29.png" data-original-width="1600" data-original-height="241" /></a>

So, as you can see it's not so difficult. We can add another field, say `howdy` and make it return a string result `universe`. To do that add another field in the `HelloWorldQuery` class as followings,

    Field<StringGraphType>(
        name: "howdy",
        resolve: context => "universe"
    ); 

Now go back to the `Starup.cs`, and this time only ask for the ***howdy*** `{ howdy }` field in the query. You will have the following response,

<a href="https://3.bp.blogspot.com/-QtL0Lhxu4Hc/WqPYBVEfswI/AAAAAAAAB0k/yMo74WQaG4wj_-Z2CUZU0UtBSxHQch-7gCLcBGAs/s1600/screencapture-localhost-5000-2018-03-10-19_04_06.png" imageanchor="1" ><img border="0" src="https://3.bp.blogspot.com/-QtL0Lhxu4Hc/WqPYBVEfswI/AAAAAAAAB0k/yMo74WQaG4wj_-Z2CUZU0UtBSxHQch-7gCLcBGAs/s1600/screencapture-localhost-5000-2018-03-10-19_04_06.png" data-original-width="1600" data-original-height="250" /></a>

And you can also ask for both fields by passing a query like the following,

    {
        hello
        howdy
    }

And you will have the following response,

<a href="https://3.bp.blogspot.com/-NayY3T0C1w8/WqPYoA8pStI/AAAAAAAAB0s/67Ftu4GmsnAhqAMhm0um6vsxOcUkV70EACLcBGAs/s1600/screencapture-localhost-5000-2018-03-10-19_06_47.png" imageanchor="1" ><img border="0" src="https://3.bp.blogspot.com/-NayY3T0C1w8/WqPYoA8pStI/AAAAAAAAB0s/67Ftu4GmsnAhqAMhm0um6vsxOcUkV70EACLcBGAs/s1600/screencapture-localhost-5000-2018-03-10-19_06_47.png" data-original-width="1600" data-original-height="285" /></a>

So, you get the point. You are asking for the things you are interested in a declarative manner. And GraphQL is intelligent enough to understand your needs and giving you back the appropriate response.  

We only scratched the surface here. This *hello world* example was needed because in the next post when we will start building the middleware; we won't have any problems understanding each and every line of the code.

#### Repository Link (Branch):

[Part I](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_I_Hello_World)

#### Important Links:

[C#: Why you should use ConfigureAwait(false) in your library code](https://medium.com/bynder-tech/c-why-you-should-use-configureawait-false-in-your-library-code-d7837dce3d7f)

[Getting Started with graphql-dotnet](https://graphql-dotnet.github.io/getting-started/)
    
[GraphQL Scaler Types](http://graphql.org/learn/schema/#scalar-types)
