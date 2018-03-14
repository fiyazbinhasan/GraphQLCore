using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace GraphQLAPI
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Run(async (context) =>
            {
                if (context.Request.Path.StartsWithSegments("/api/graphql") && string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
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
        }
    }

    public class GraphQLRequest
    {
        public string Query { get; set; }
    }
}
