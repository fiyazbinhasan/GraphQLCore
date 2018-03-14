# GraphQL with ASP.NET Core (Part- II : Middleware)

If you are familiar with ASP.NET Core ***middleware***<sup>[[1]](#middleware)</sup>, you may have noticed that in our previous post we already had a middleware. In the initial blank app, that middleware was responsible for throwing a `Hello World` response. Later we replaced it with our custom code so that it can respond back a result of some static `GraphQL` query.

<blockquote id="middleware">
<p>Middleware is software that's assembled into an application pipeline to handle requests and responses. Each component:</p>
<ul>
<li>Chooses whether to pass the request to the next component in the pipeline.</li>
<li>Can perform work before and after the next component in the pipeline is invoked.</li>
</ul>
<a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?tabs=aspnetcore2x#what-is-middleware"><p>- Microsoft Documentation</p></a>
</blockquote>

Practically a middleware is a delegate or more precisely; a `request delegate`. As the name suggests, it handles incoming request and decides whether or not to delegate it to the next middleware in the pipeline. In our case, we configured a request delegate using the `Run()` (***extension***) method of `IApplicationBuider`. Between three extension methods (`Use`, `Run`, `Map`), `Run()` terminates the request for further modifications in the request pipeline.  

The code inside our middleware was very simple and can only response back result of a hardcoded static query. However, in a real world scenario the query should be dynamic hence we must read it from the incoming request.   

Every request delegate accepts a `HttpContext`. If the query is posted over a http request, you can easily read the request body using the following code,

```
string body;
using (var streamReader = new StreamReader(httpContext.Request.Body))
{
    body = await streamReader.ReadToEndAsync();
}
```

Validating the request first before reading its content won't do any harm. So, let's put an `if` clause that checks two things,

* Is it a `POST` request?
* is the `POST` request is made to a specific URL i.e. `api/graphql`? 

So, the code will be modified as follows,

```
if(context.Request.Path.StartsWithSegments("/api/graphql") && string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
{
    string body;
    using (var streamReader = new StreamReader(context.Request.Body))
    {
        body = await streamReader.ReadToEndAsync();
    }

....
....
....
```

A request body can contain a whole lot of fields, but let's say the passed in query comes within a field named `query`. So we can parse out the json string content of the `body` into a complex type that contains a `Query` property,

The complex type looks as follows,

    public class GraphQLRequest
    {
        public string Query { get; set; }
    }

Next thing to do is deserialize the `body` to an instance of `GraphQLRequest` type using `Json.Net`'s  `JsonConvert.DeserializeObject` and replace the previous hardcoded query with the `request.Query`,

    var request = JsonConvert.DeserializeObject<GraphQLRequest>(body);

    
    var result = await new DocumentExecuter().ExecuteAsync(doc =>
    {
        doc.Schema = schema;
        doc.Query = request.Query;
    }).ConfigureAwait(false);

So, after all the modifications, code in the `Run` method of  `Startup.cs` looks as follows,

    app.Run(async (context) =>
    {
        if (context.Request.Path.StartsWithSegments("/api/graphql")
       && string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
        {
            string body;
            using (var streamReader = new StreamReader(context.Request.Body))
            {
                body = await streamReader.ReadToEndAsync();
            
                var request = JsonConvert.DeserializeObject<GraphQLRequest>(body);
                var schema = new Schema { Query = new HelloWorldQuery() };
            
                var result = await new DocumentExecuter().ExecuteAsync(doc =>
                {
                    doc.Schema = schema;
                    doc.Query = request.Query;
                }).ConfigureAwait(false);

                var json = new DocumentWriter(indent: true).Write(result);
                await context.Response.WriteAsync(json);
            }
        }
    });

Now you make a `POST` request containing the query field using any rest client (Postman/Insomnia), 

<a href="https://2.bp.blogspot.com/-8U4Za22lx8E/Wqk8aN7MqQI/AAAAAAAAB1I/al6wFP4hMpEheJvNPlp7bn7vGMtY5nBEACLcBGAs/s1600/Screen%2BShot%2B2018-03-14%2Bat%2B9.13.00%2BPM.png" imageanchor="1" ><img border="0" src="https://2.bp.blogspot.com/-8U4Za22lx8E/Wqk8aN7MqQI/AAAAAAAAB1I/al6wFP4hMpEheJvNPlp7bn7vGMtY5nBEACLcBGAs/s1600/Screen%2BShot%2B2018-03-14%2Bat%2B9.13.00%2BPM.png" data-original-width="1600" data-original-height="381" /></a>

We are pretty much done with this post. But you can see that we have a lot of `newing` ups of objects like the `new DocumentExecuter()`, `new Schema()`, `new DocumentWriter()` etc. In the next post we will see how we can use the built-in dependency system of `ASP.NET Core` and make them injectable.

#### Repository Link (Branch)

[Part II](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_II_Middleware)

#### Important Links

[ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?tabs=aspnetcore2x)
