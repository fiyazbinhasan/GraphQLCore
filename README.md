# GraphQL with ASP.NET Core (Part- VI : Persist Data - Postgres with EF Core)

This post focuses more on configuring a persistent data storage rather than discussing different aspects of GraphQL. With that being said, let's connect a `Postgres` database with our back-end. You may ask, why `Postgres`? Because everybody does SQL Server; so why not try out a different thing.

In our data access layer, we will have a data store class or in another word a repository class. Since it's a good practice to code against abstraction; we will create an interface first for the store class i.e. IDataStore

    public interface IDataStore
    {
        IEnumerable<Item> GetItems();
        Item GetItemByBarcode(string barcode);
    }

We are already familiar with the `GetItemByBarcode` method. The `GetItems` returns all the items in the inventory. We will add a `GraphQL` collection field for that later. 

The implementation of the `IDataStore` is pretty simple as following,

    public class DataStore : IDataStore
    {
        private ApplicationDbContext _applicationDbContext;

        public DataStore(ApplicationDbContext applicationDbContext)
        {
		_applicationDbContext = applicationDbContext;
        }

        public Item GetItemByBarcode(string barcode)
        {
		return _applicationDbContext.Items.First(i => i.Barcode.Equals(barcode));
        }

        public IEnumerable<Item> GetItems()
        {
		return _applicationDbContext.Items;
        }
    }

We are using entity framework core, hence the introduction of `ApplicationDbContext`. The class extends from the `DbContext` of entity framework and contains a single `DbSet` for the `Item` entity. It will create a table named `Items` once we run the migration,

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Item> Items { get; set; }
    }

`DbContextOptions` is a cool way to pass parameter such as `ConnectionString` while configuring `ApplicationDbContext` inside `ConfigureServices` method of `Startup.cs`.


```
services.AddEntityFrameworkNpgsql().AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Configuration["DefaultConnection"]));
```

The `AddEntityFrameworkNpgsql()` entension comes from a seperate package i.e. `Npgsql.EntityFrameworkCore.PostgreSQL`. Install this via `nuget` or `dotnet cli`

> dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 2.1.0
 
The `Configuration` property is a type of `IConfigurationRoot`. We build a configuration object and assign that to the `Configuration` property in the constructor of `Startup.cs`.

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

You can store the connections string in the `appsettings.json` file. For me, I always store them in the user secrets file for security reasons, hence `builder.AddUserSecrets<Startup>()`. You can add your connection string in the user secret file with the following command,

> dotnet user-secrets set DefaultConnection 'your-connection-string'

Add an initial migration with the following command in the terminal,

> dotnet ef migrations add Initial --output-dir Data\Migrations

Apply the migration to create your database with the following command,

> dotnet ef database update

`AddDbContext<ApplicationDbContext>()` registers the `DbContext` with a `scoped` service lifetime. Difference between singleton and scope lifetime are, 

* A `Singleton` service instance is created only one time (when the application first starts) and the same instance is shared with other services for every subsequent request.

* A `Scope` service instance is created everytime a new request comes in. It's like `singleton per request`.

Until now, we've been registering every service with a singleton lifetime. But if we do the same for the `IDataStore` there will be some consequences,

* If you notice carefully, we are injecting `ApplicationDbContext` directly in the `DataStore`. More simply, we are accessing a `scoped` service from a `singleton` service. 

* Even though `scope` service instances are created per request; since we are accessing it from a `singleton` lifetime it will always return the first instance that stays with it. Hence making it behave like a singleton too.

So, we must register the `IDataStore` with a `scoped` lifetime as well,

    services.AddScoped<IDataStore, DataStore>();

As of now, with EF Core 2.0; we still don't have any default `Seed` method. So, the following class does a minimal job of seeding a database,

    public class ApplicationDatabaseInitializer
    {
        public async Task SeedAsync(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                await applicationDbContext.Database.EnsureDeletedAsync();
                await applicationDbContext.Database.MigrateAsync();
                await applicationDbContext.Database.EnsureCreatedAsync();

                var items = new List<Item>
                {
                    new Item { Barcode= "123", Title="Headphone", SellingPrice=50},
                    new Item { Barcode= "456", Title="Keyboard", SellingPrice= 40},
                    new Item { Barcode= "789", Title="Monitor", SellingPrice= 100}
                };

                await applicationDbContext.Items.AddRangeAsync(items);

                await applicationDbContext.SaveChangesAsync();
            }
        }
    }

We need to seed the database once the application starts. So, add the following line in the `Configure` method,

    new ApplicationDatabaseInitializer().SeedAsync(app).GetAwaiter();

> We don't require any data seeding technique once we step into production. That's why I'm not so bothered about `newing` up `ApplicationDatabaseInitializer` inside the `Configure` method.

Other modifications include changing service lifetime for `HelloWorldQuery` and `HelloWorldSchema` to scope.

```
services.AddScoped<HelloWorldQuery>();
services.AddScoped<ISchema, HelloWorldSchema>();
```

At this moment, your application will run but it ***won't*** be able to register the schema. Always remember that a .net core middleware is registered only once when the application first starts. But to use a `scoped`/`transient` service inside a middleware we have to inject the service via the `InvokeAsync()` method,

    public async Task InvokeAsync(HttpContext httpContext, ISchema schema)
    {
        ....
        ....
    }

One last thing I want to do is to add a new collection field for showing all the items. The type of the field would be a `ListGraphType` of `ItemType`,

    Field<ListGraphType<ItemType>>(
        "items",
        resolve: context =>
        {
            return dataStore.GetItems();
        }
    );

Run the application now and try to query the `items` field and you will see something like the following,

<a href="https://1.bp.blogspot.com/-y0awsM-MDXc/WvQdIvdEPVI/AAAAAAAAB5I/DF5Ygg2aYOQQJWCJHr9t7es9YoWdDmy6wCLcBGAs/s1600/GraphiQL.png" imageanchor="1" ><img border="0" src="https://1.bp.blogspot.com/-y0awsM-MDXc/WvQdIvdEPVI/AAAAAAAAB5I/DF5Ygg2aYOQQJWCJHr9t7es9YoWdDmy6wCLcBGAs/s1600/GraphiQL.png" data-original-width="1600" data-original-height="525" /></a>

#### Repository Link (Branch)

[Part VI](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_VI_Persist_Data)

#### Important Links


[Getting started with PostgreSQL with EF Core](http://www.npgsql.org/efcore/index.html)

[Don't Share Your Secrets! (.NET CORE Secret Manager Tool)](http://fiyazhasan.me/dont-share-your-secrets-asp-net-core-secret-manager-tool/)

[GraphQL Schema and Type](https://graphql.org/learn/schema/)
