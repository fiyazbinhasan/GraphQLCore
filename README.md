# GraphQL with ASP.NET Core (Part- X : Data Loader - Series Finale)

Our `GraphQL` queries are not quite optimized. Take the `Orders` query from `CustomerType` for example,

*CustomerType.cs*

```
Field<ListGraphType<OrderType>, IEnumerable<Order>>()
    .Name("Orders")
    .ResolveAsync(ctx =>
    {
	    return dataStore.GetOrdersAsync();
    }); 
```

Here, we are getting all the orders from the data store. This is all fun and games till you stay in the scaler zone of `OrderType` i.e. only querying the scaler properties of `OrderType`. But what happens when you query for one of the navigational property. For example, code in the `OrderType` is as following,

*OrderType.cs*

```
public OrderType(IDataStore dataStore, IDataLoaderContextAccessor accessor)
{
	Field(o => o.Tag);
	Field(o => o.CreatedAt);
	Field<CustomerType, Customer>()
		.Name("Customer")
		.ResolveAsync(ctx =>
	    {            
		    return dataStore.GetCustomerByIdAsync(ctx.Source.CustomerId);  
	    });
}
```

So, when you try to access the `Customer` field, practically you are initiating a separate request to your data store to load the related customer for a particular order.

If you are using the dotnet cli, you can actually see all the EF query logs in the console for a query such as,

```
{
  orders{
    tag
    createdAt
    customer{
      name
      billingAddress
    }
  }
}
``` 
*Logs*
```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (3ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "o"."OrderId", "o"."CreatedAt", "o"."CustomerId", "o"."Tag"
      FROM "Orders" AS "o"
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (9ms) [Parameters=[@__get_Item_0='?'], CommandType='Text', CommandTimeout='30']
      SELECT "e"."CustomerId", "e"."BillingAddress", "e"."Name"
      FROM "Customers" AS "e"
      WHERE "e"."CustomerId" = @__get_Item_0
      LIMIT 1
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[@__get_Item_0='?'], CommandType='Text', CommandTimeout='30']
      SELECT "e"."CustomerId", "e"."BillingAddress", "e"."Name"
      FROM "Customers" AS "e"
      WHERE "e"."CustomerId" = @__get_Item_0
      LIMIT 1
```

The logs very well suggest that; first, we are querying for all the orders and then for each order, we are querying for the customer as well. Here, for `2` orders we have `2 + 1 = 3` queries (total 3 hits on the database). Now, do your math and figure out how many times we will hit the database if we have N numbers of orders. Well, we will have a total `N + 1` queries hence, the problem is named `N + 1` problem. 

To overcome this problem, we introduce `DataLoader` in our solution. `DataLoader` adds support for [batching](https://github.com/facebook/dataloader#batching) and [caching](https://github.com/facebook/dataloader#caching) in your `GraphQL` queries. 

Adding support for `DataLoader` needs some configurations up front. Register the `IDataLoaderContextAccessor` and `DataLoaderDocumentListener` with a singleton lifetime in your `ConfigureServices` method,

*Startup.cs*

```
services.AddSingleton<IDataLoaderContextAccessor, DataLoaderContextAccessor>();
services.AddSingleton<DataLoaderDocumentListener>();
```

`IDataLoaderContextAccessor` will be injected later in the constructors of graph types where data loader is needed. But first, in the middleware; we have to add the `DataLoaderDocumentListener` to the list of listeners of `IDocumentExecutor`'s `ExecutionOptions`. 

*GraphQLMiddleware.cs*

```
public async Task InvokeAsync(HttpContext httpContext, ISchema schema, IServiceProvider serviceProvider)  
{
    ....
    ....
            var result = await _executor.ExecuteAsync(doc =>
            {
                ....
                ....

                doc.Listeners.Add(serviceProvider.GetRequiredService<DataLoaderDocumentListener>());

            }).ConfigureAwait(false);

    ....
    ....            
}
```

Next, add a new method to your datastore which takes a list of customer ids and returns a dictionary of customers with their ids as keys.

*DataStore.cs*

```
public async Task<Dictionary<int, Customer>> GetCustomersByIdAsync(IEnumerable<int> customerIds, CancellationToken token)
{
    return await _applicationDbContext.Customers.Where(i => customerIds.Contains(i.CustomerId)).ToDictionaryAsync(x => x.CustomerId);
}
```

You can replace the `Customer` field with the following,


*OrderType.cs*

```
Field<CustomerType, Customer>()  
    .Name("Customer")
    .ResolveAsync(ctx =>
    {            
        var customersLoader = accessor.Context.GetOrAddBatchLoader<int, Customer>("GetCustomersById", dataStore.GetCustomersByIdAsync);
        return customersLoader.LoadAsync(ctx.Source.CustomerId);  
    });
```

> Idea behind `GetOrAddBatchLoader` is that it waits until all the customer ids are queued. Then it fires of the `GetCustomersByIdAsync` method only once with all the collected ids. Once the dictionary of customers is returned with the passed in ids; a customer that belongs to a particular order is returned from the field with some internal object mapping. Remember, this technique of queueing up ids is called batching. We will always have a single request to load related customers for orders no matter what i.e. we will at most have 2 requests.

Running the application and firing the same query as before will provide you the following query logs.

*Logs*

```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (3ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "o"."OrderId", "o"."CreatedAt", "o"."CustomerId", "o"."Tag"
      FROM "Orders" AS "o"
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (10ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "i"."CustomerId", "i"."BillingAddress", "i"."Name"
      FROM "Customers" AS "i"
      WHERE "i"."CustomerId" IN (1, 2)
```

Notice the second query. See how it queries for all the customers with the incoming ids. 

Similarly, for a collection navigation property, you have `GetOrAddCollectionBatchLoader`. Take the `Orders` field of the `CustomerType` for example. You add a new data store method as following,

*DataStore.cs*
```
public async Task<ILookup<int, Order>> GetOrdersByCustomerIdAsync(IEnumerable<int> customerIds)  
{
    var orders = await _applicationDbContext.Orders.Where(i => customerIds.Contains(i.CustomerId)).ToListAsync();
            return orders.ToLookup(i => i.CustomerId);
}
```

Notice, here we are returning an `ILookup` data structure instead of a dictionary. The only difference between them is `ILookup` can have multiple values against a single key whereas for the dictionary; a single key belongs to a single value.

Modify the `Orders` value inside the `CustomerType` as following,

*CustomerType.cs*

```
Field<ListGraphType<OrderType>, IEnumerable<Order>>()  
    .Name("Orders")
    .ResolveAsync(ctx => 
    {
        var ordersLoader = accessor.Context.GetOrAddCollectionBatchLoader<int, Order>("GetOrdersByCustomerId", dataStore.GetOrdersByCustomerIdAsync);
        return ordersLoader.LoadAsync(ctx.Source.CustomerId);
    });
```

`GetOrAddCollectionBatchLoader` and `GetOrAddBatchLoader` both caches the values of the field for the lifetime of a `GraphQl` query. If you only want to use the caching feature and ignore batching, you can simply use the `GetOrAddLoader`. 

Caching is good for fields you request too frequently. So, you can add caching feature in your `Items` field of the `InventoryQuery` as following,

*InventoryQuery.cs*

```
Field<ListGraphType<ItemType>, IEnumerable<Item>>()  
    .Name("Items")
    .ResolveAsync(ctx =>
    {
        var loader = accessor.Context.GetOrAddLoader("GetAllItems", () => dataStore.GetItemsAsync());
        return loader.LoadAsync();
    });
```

#### Repository Link (Branch)

[Part X](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_X_DataLoader)

#### Important Links

[GraphQl-Dontnet DataLoader](https://graphql-dotnet.github.io/dataloader/)

[Batching GraphQL Queries with DataLoader](Batching GraphQL Queries with DataLoader)
 
