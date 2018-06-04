# GraphQL with ASP.NET Core (Part- VIII : Entity Relations - One to Many)

Building a `GraphQL` end-point with a single entity ain't gonna cut it. In this post, we introduce two new entities for handling orders for a customer. The relationship between `Customer` and `Order` is one-to-many i.e. A customer can have one or many orders, whereas a particular order belongs to a single customer.

You can configure entity relationship following entity framework conventions. Entity framework will auto-create a one-to-many relationship between entities if one of the entity contains a collection property of the second entity. This property is known as a ***navigation property***<sup>[[1]](#navigation)</sup>.

In `Customer` entity you have `Orders` as a collection navigation property. 

*Customer.cs*

```
public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; }
    public string BillingAddress { get; set; }
    public IEnumerable<Order> Orders { get; set; }
}
```

Most of the time a collection navigation property is enough to declare a one-to-many relationship. However, it is suggested that you declare a fully defined relationship. To achieve that, on the second entity you define a foreign key property along with a reference navigation property. 

Following represents the `Order` entity where `CustomerId` is a foreign key and the `Customer` is a reference navigation property.

*Order.cs*

```
public class Order
{
    public int OrderId { get; set; }
    public string Tag { get; set;}
    public DateTime CreatedAt { get; set;}

    public Customer Customer { get; set; }
    public int CustomerId { get; set; }
}
```

<blockquote id="navigation">
<p>A property is considered a navigation property if the type it points to can not be mapped as a scalar type by the current database provider.- <a href="https://docs.microsoft.com/en-us/ef/core/modeling/relationships#conventions">docs.microsoft.com</a></p>
</blockquote>

Once you have configured all the necessary relationships, create a migration and update your database with dotnet CLI,

```
dotnet ef migrations add OneToManyRelationship
dotnet ef database update
```

I've added two new `ObjectGraphType`s for defining accessible fields of `Order` and `Customer` as followings,

*OrderType.cs*

```
public class OrderType: ObjectGraphType <Order> {
    public OrderType(IDataStore dataStore) {
        Field(o => o.Tag);
        Field(o => o.CreatedAt);
        Field <CustomerType, Customer> ()
            .Name("Customer")
            .ResolveAsync(ctx => {
                return dataStore.GetCustomerByIdAsync(ctx.Source.CustomerId);
            });
    }
}
```

*CustomerType.cs*

```
public class CustomerType: ObjectGraphType <Customer> {
    public CustomerType(IDataStore dataStore) {
        Field(c => c.Name);
        Field(c => c.BillingAddress);
        Field <ListGraphType<OrderType> , IEnumerable <Order>> ()
            .Name("Orders")
            .ResolveAsync(ctx => {
                return dataStore.GetOrdersByCustomerIdAsync(ctx.Source.CustomerId);
            });
    }
}
```

To expose two new end-points for accessing all the customers and orders, I've registered two new fields of `ListGraphType` inside the `InventoryQuery` as following,

*InventoryQuery.cs*

```
Field<ListGraphType<OrderType>, IEnumerable<Order>>()
    .Name("Orders")
    .ResolveAsync(ctx =>
    {
	    return dataStore.GetOrdersAsync();
    });

Field<ListGraphType<CustomerType>, IEnumerable<Customer>>()
    .Name("Customers")
    .ResolveAsync(ctx =>
    {
        return dataStore.GetCustomersAsync();
    });
```

Implementations of the newly added fetch methods inside `DataStore.cs` are as following,

*DataStore.cs*

```
public async Task <IEnumerable<Order>> GetOrdersAsync() {
    return await _applicationDbContext.Orders.AsNoTracking().ToListAsync();
}

public async Task <IEnumerable<Customer>> GetCustomersAsync() {
    return await _applicationDbContext.Customers.AsNoTracking().ToListAsync();
}

public async Task <Customer> GetCustomerByIdAsync(int customerId) {
    return await _applicationDbContext.Customers.FindAsync(customerId);
}

public async Task <IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId) {
    return await _applicationDbContext.Orders.Where(o => o.CustomerId == customerId).ToListAsync();
}
```

I've also threw in two additional methods for creating `Customer` and `Order`,

```
public async Task<Order> AddOrderAsync(Order order)
{
    var addedOrder = await _applicationDbContext.Orders.AddAsync(order);
    await _applicationDbContext.SaveChangesAsync();
    return addedOrder.Entity;
}

public async Task<Customer> AddCustomerAsync(Customer customer)
{         
    var addedCustomer = await _applicationDbContext.Customers.AddAsync(customer);
    await _applicationDbContext.SaveChangesAsync();
	return addedCustomer.Entity;
}
```

Remember the last post on mutation, you had to create a new `InputObjectGraphType` for `Item` in order to create side effects. Likewise followings are the `InputObjectGraphType` for `Customer` and `Order`.

*OrderInputType.cs*

```
public class OrderInputType : InputObjectGraphType {
	public OrderInputType()
	{
		Name = "OrderInput";
		Field<NonNullGraphType<StringGraphType>>("tag");
		Field<NonNullGraphType<DateGraphType>>("createdAt");
		Field<NonNullGraphType<IntGraphType>>("customerId");
	}
}
```

*CustomerInputType.cs*

```
public class CustomerInputType : InputObjectGraphType {
	public CustomerInputType()
	{
		Name = "CustomerInput";
		Field<NonNullGraphType<StringGraphType>>("name");
		Field<NonNullGraphType<StringGraphType>>("billingAddress");
	}
}
```

Finally, we need to register all the types with the DI system. Newly created services registration inside `ConfigureServices` are as followings,

*Startup.cs*

```
public void ConfigureServices(IServiceCollection services)
{ 
....
....
    services.AddScoped<CustomerType>();
    services.AddScoped<CustomerInput>();
    services.AddScoped<OrderType>();
    services.AddScoped<OrderInputType>();
}
```
If you run the application now, we will have the following error message,

> "No parameterless constructor defined for this object."

<a href="https://4.bp.blogspot.com/-AOg-TIulONg/WxV_3ieUNUI/AAAAAAAAB7E/wFYSNtdFkqsIANe7gUjP1dRqfJUiIQ9RQCLcBGAs/s1600/error.png" imageanchor="1" ><img border="0" src="https://4.bp.blogspot.com/-AOg-TIulONg/WxV_3ieUNUI/AAAAAAAAB7E/wFYSNtdFkqsIANe7gUjP1dRqfJUiIQ9RQCLcBGAs/s1600/error.png" data-original-width="1600" data-original-height="411" /></a>

So, after digging through the repository of `graphql-dotnet`, I found this [issue](https://github.com/graphql-dotnet/graphql-dotnet/issues/616) regarding the problem.

Turns out, constructor injection with the `Schema` doesn't work the way I assumed it would have. With the current solution, the DI system can resolve a type once and it ***can't*** resolve it again for other graph types down the graph chain. In short, if you inject the `IDataStore` once in the constructor of your `InventoryQuery`; you are pretty much done. You ***can't*** inject it in the constructor of other graph types; for example in the `CustomerType`. But this is not the behavior we want. Hence, come forth `IDependencyResolver`. Register `IDependencyResolver` with the DI system and be sure to give it a scoped lifetime.

```
services.AddScoped<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));
```

A simple modification is needed on `InventorySchema.cs`. Change the code and inject `IDependencyResolver` inside the constructor as following,

*InventorySchema.cs*

```
public class InventorySchema: Schema {
    public InventorySchema(IDependencyResolver resolver): base(resolver) {
        Query = resolver.Resolve < InventoryQuery > ();
        Mutation = resolver.Resolve < InventoryMutation > ();
    }
}
```

Now, run the application and make sure you can access the newly added fields,


<a href="https://1.bp.blogspot.com/-y8-LcxzBvTI/WxWFSe03YpI/AAAAAAAAB7Q/0dVa8QEX9iUEc-0nYoG2ZjJ9F60LwpjqQCLcBGAs/s1600/query.png" imageanchor="1" ><img border="0" src="https://1.bp.blogspot.com/-y8-LcxzBvTI/WxWFSe03YpI/AAAAAAAAB7Q/0dVa8QEX9iUEc-0nYoG2ZjJ9F60LwpjqQCLcBGAs/s1600/query.png" data-original-width="1600" data-original-height="744" /></a>

#### Repository Link (Branch)

[Part VIII](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_VIII_Entity_Relations_One_To_Many)

#### Important Links

[Modeling EF Core Relationships](https://docs.microsoft.com/en-us/ef/core/modeling/relationships)

[Tracking vs No-Tracking Queries](https://docs.microsoft.com/en-us/ef/core/querying/tracking)
