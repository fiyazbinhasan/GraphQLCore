using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using GraphQLAPI.Data;
using GraphQLAPI.Middleware;
using GraphQLAPI.Store;
using GraphQLAPI.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace GraphQLAPI
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDocumentWriter, DocumentWriter>();
            services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
           
			services.AddScoped<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));

			services.AddScoped<HelloWorldQuery>();
			services.AddScoped<ISchema, HelloWorldSchema>();

			services.AddScoped<ItemType>();
			services.AddScoped<IDataStore, DataStore>();

            services.AddEntityFrameworkNpgsql().AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Configuration["DefaultConnection"]));

            services.AddScoped<IApplicationDatabaseInitializer, ApplicationDatabaseInitializer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationDatabaseInitializer applicationDatabaseInitializer)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();

			app.UseMiddleware<GraphQLMiddleware>();
            applicationDatabaseInitializer.SeedAsync(app);
        }
    }
}
