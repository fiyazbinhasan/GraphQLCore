using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using GraphQL;
using GraphQL.Http;
using Newtonsoft.Json;
using System.IO;
using GraphQL.Types;

namespace GraphQLAPI
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class GraphQLMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDocumentExecuter _executer;
        private readonly IDocumentWriter _writer;

        public GraphQLMiddleware(RequestDelegate next, IDocumentExecuter executer, IDocumentWriter writer)
        {
            _next = next;
            _executer = executer;
            _writer = writer;
        }

        public async Task InvokeAsync(HttpContext httpContext, ISchema schema)
        {
            if(httpContext.Request.Path.StartsWithSegments("/api/graphql")
                && string.Equals(httpContext.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                string body;
                using (var streamReader = new StreamReader(httpContext.Request.Body))
                {
                    body = await streamReader.ReadToEndAsync();
                }

                var request = JsonConvert.DeserializeObject<GraphQLRequest>(body);
                
                var result = await _executer.ExecuteAsync(doc =>
                {
                    doc.Schema = schema;
                    doc.Query = request.Query;
                });

                var json = _writer.Write(result);
                await httpContext.Response.WriteAsync(json);
            }
            else
            {
                await _next(httpContext);
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class GraphQLMiddlewareExtensions
    {
        public static IApplicationBuilder UseGraphQLMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GraphQLMiddleware>();
        }
    }

    public class GraphQLRequest
    {
        public string Query { get; set; }
    }
}
