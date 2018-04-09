# GraphQL with ASP.NET Core (Part- III : Dependency Injection)

The letter 'D' in [SOLID](https://en.wikipedia.org/wiki/SOLID_(object-oriented_design)) stands for `Dependency inversion principle`. The principle states,

> * A. High-level modules should not depend on low-level modules. Both should depend on abstractions.
> * Abstractions should not depend on details. Details should depend on abstractions. [Wikipedia](https://en.wikipedia.org/wiki/Dependency_inversion_principle)

Newing up instances cause strict coupling between separate code modules. To keep them decoupled from each other, we follow the 'Dependency Inversion Principle'. In this way the modules are not dependent on each other's concrete implementation rather they are dependent upon abstractions e.g. interfaces.

An abstraction can have many many implementations. So, whenever we encounter an abstraction, there should be some way of passing a specific implementation to that. A class is held responsible for this kind of servings and it should be configured in such way so. We call it a dependency injection container.

ASP.Net Core has its built-in dependency injection container. It's simple and can serve our purpose very well. Not only it can be configured to serve implementations to abstractions but also can control the lifetime of the created instances. 

Currently with our simple `Hello World` application, we are not bothered with instance lifetime. As of now we will go for `Singleton` instance lifetime for everything,

Instead of using the concrete `DocumentWriter` and `DocumentExecutor`, we can use their abstractions i.e. `IDocumentWriter` and `DocumentExecutor`. And for this purpose we have to configure the built-in dependency container as follows,

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IDocumentWriter, DocumentWriter>();
    services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
}
```

For the `HelloWordQuery`, we don't have any abstraction. We will just use the raw implementation,

```
services.AddSingleton<HelloWordQuery>();
```

The schema contains the `query` and later in the series it will also have `mutation` and other fields. We better make a separate class for it. The class will extend the `Schema` type and we can make its constructor injectable for the concrete `HelloWorldQuery`,


    public class HelloWorldSchema : Schema
    {
        public HelloWorldSchema(HelloWorldQuery query)
        {
            Query = query;
        }
    }

Finally it's time to configure the `HelloWorldSchema` in the `ConfigureServices` method as following,


```
services.AddSingleton<ISchema, HelloWorldSchema>();
```

> The `ISchema` is coming from the `graphql-dotnet` library itself.

Now, we can shift the middleware code to its own class. Following is the middleware class named `GraphQLMiddleware`,

    public class GraphQLMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDocumentWriter _writer;
        private readonly IDocumentExecuter _executor;
        private readonly ISchema _schema;

        public GraphQLMiddleware(RequestDelegate next, IDocumentWriter writer, IDocumentExecuter executor, ISchema schema)
        {
            _next = next;
            _writer = writer;
            _executor = executor;
            _schema = schema;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.Request.Path.StartsWithSegments("/api/graphql") && string.Equals(httpContext.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                string body;
                using (var streamReader = new StreamReader(httpContext.Request.Body))
                {
                    body = await streamReader.ReadToEndAsync();

                    var request = JsonConvert.DeserializeObject<GraphQLRequest>(body);

                    var result = await _executor.ExecuteAsync(doc =>
                    {
                        doc.Schema = _schema;
                        doc.Query = request.Query;
                    }).ConfigureAwait(false);

                    var json = _writer.Write(result);
                    await httpContext.Response.WriteAsync(json);
                }
            }
            else
            {
                await _next(httpContext);
            }
        }
    }

Notice, how we replace all the concrete type initialisations with abstractions and make our code loose coupled. Every dependency injectable service in injected via the constructor (constructor injection) at this moment.

The last but not least, we must attach the middleware in the application startup pipeline. `IApplicationBuilder` has an extension method called `UseMiddleware` which is used to attach middleware classes. So, the final look of the `Configure` method is as follows,

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        app.UseMiddleware<GraphQLMiddleware>();
    }

#### Repository Link (Branch)

[Part III](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_III_Dependency_Injection)

#### Important Links

[Dependency injection in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)

[Dependency injection to the core](http://fiyazhasan.me/tag/dependency-injection-2/)

