# GraphQL with ASP.NET Core (Part- IX : Entity Relations - Many to Many)

We have an opportunity to show off a many-to-many relation in our sample app and I don't want to miss that. In a real-world scenario, only a limited quantity of a particular item belongs to a particular order. Imagine, you have an order cart containing references to some selected items with their respective ordered quantities. The end result is you got a relationship something like: an order can have many items whereas an item can be part of multiple orders. 

By EF conventions, in a many-to-many relation, you have two standalone entities and a third entity between them which represents a relationship bridge. In our case, the bridging entity will be the `OrderItem`.

*OrderItem.cs*

```
public class OrderItem  
{
    public int Id { get; set; }

    public int ItemId { get; set; }
    public Item Item { get; set; }

    public int Quantity { get; set; }      

    public int OrderId { get; set; }
    public Order Order { get; set; }
}
```

Here we have two reference navigation properties for each side of the relation i.e. `Order` and `Item`. Also, we have two foreign key properties i.e. `ItemId` and `OrderId`.

For a fully defined relationship, we also have  individual collection navigation property of `OrderItem` on each side of the standalone entities,

*Order.cs*

```
public class Order  
{
    public int OrderId { get; set; }
    public string Tag { get; set;}
    public DateTime CreatedAt { get; set;}

    public Customer Customer { get; set; }
    public int CustomerId { get; set; }

    public IEnumerable<OrderItem> OrderItems { get; set; }
}
```

*Item.cs*

```
public class Item  
{
    public int ItemId { get; set; }
    public string Barcode { get; set; }
    public string Title { get; set; }
    public decimal SellingPrice { get; set; }

    public IEnumerable<OrderItem> OrderItems { get; set; }
}
```

Once you have configured all the necessary relationships, create a migration and update your database with dotnet CLI,

```
dotnet ef migrations add ManyToManyRelationship  
dotnet ef database update  
```

Now, we can add a `GraphQL` end-point for adding an item to a particular order. To do that we need an `InputGraphTypeType` for `OrderItem`.

*OrderItemInputType.cs*

```
public class OrderItemInputType : InputObjectGraphType  
{
    public OrderItemInputType()
    {
        Name = "OrderItemInput";
        Field<NonNullGraphType<IntGraphType>>("quantity");
        Field<NonNullGraphType<IntGraphType>>("itemId");
        Field<NonNullGraphType<IntGraphType>>("orderId");
    }
}
```

As for the end-point, we registered a new mutation field inside `InventoryMutation.cs`. The field is simply named `addOrderItem`,

*InventoryMutation.cs*

```
Field<OrderItemType, OrderItem>()  
    .Name("addOrderItem")
    .Argument<NonNullGraphType<OrderItemInputType>>("orderitem", "orderitem input")
    .ResolveAsync(ctx =>
    {
        var orderItem = ctx.GetArgument<OrderItem>("orderitem");
        return dataStore.AddOrderItemAsync(orderItem);
    });
```

Newly added `OrderItemType` is as following,

*OrderItemType.cs*

```
public class OrderItemType : ObjectGraphType<OrderItem>  
{
    public OrderItemType(IDataStore dateStore)
    {   
        Field(i => i.ItemId);      

        Field<ItemType, Item>().Name("Item").ResolveAsync(ctx =>
        {
            return dateStore.GetItemByIdAsync(ctx.Source.ItemId);
        });         

        Field(i => i.Quantity);

        Field(i => i.OrderId);

        Field<OrderType, Order>().Name("Order").ResolveAsync(ctx =>
        {
            return dateStore.GetOrderByIdAsync(ctx.Source.OrderId);
        });
    }
}
```

Newly registered methods from `DataStore.cs` are as folliwng,

*DataStore.cs*

```
public async Task<Item> GetItemByIdAsync(int itemId)  
{
    return await _applicationDbContext.Items.FindAsync(itemId);
}

public async Task<Order> GetOrderByIdAsync(int orderId)  
{
    return await _applicationDbContext.Orders.FindAsync(orderId);
}

public async Task<OrderItem> AddOrderItemAsync(OrderItem orderItem)  
{         
    var addedOrderItem = await _applicationDbContext.OrderItem.AddAsync(orderItem);
    await _applicationDbContext.SaveChangesAsync();
    return addedOrderItem.Entity;
}
```

I've also threw in an additional field for querying a list of all the `OrderItem` at once,

*InventoryQuery.cs*

```
Field<ListGraphType<OrderItemType>, IEnumerable<OrderItem>>()  
    .Name("OrderItem")
    .ResolveAsync(ctx =>
    {
        return dataStore.GetOrderItemAsync();
    });
```

Repository code for `GetOrderItemAsync` is as following,

*DataStore.cs*

```
public async Task<IEnumerable<OrderItem>> GetOrderItemAsync()  
{         
    return await _applicationDbContext.OrderItem.AsNoTracking().ToListAsync();
}
```

Last but not least, don't forget to register the newly added graph types with the DI system. Services registration inside `ConfigureServices` are as followings, 

*Startup.cs*

```
services.AddScoped<OrderItemType>();
services.AddScoped<OrderItemInputType>();
```

That's all about it. Run the application and try to add an item to a particular order with a mutation like the following illustration,

<a href="https://3.bp.blogspot.com/-74IkmKByvFk/Wx_3Mx4YnxI/AAAAAAAAB7g/_54YXzRIFtU4vjOaiI3CV0cKQNO6_MewACLcBGAs/s1600/addOrderItem.png" imageanchor="1" ><img border="0" src="https://3.bp.blogspot.com/-74IkmKByvFk/Wx_3Mx4YnxI/AAAAAAAAB7g/_54YXzRIFtU4vjOaiI3CV0cKQNO6_MewACLcBGAs/s1600/addOrderItem.png" data-original-width="1600" data-original-height="475" /></a>

We can also query all the `InventoryItem` with the following query,

<a href="https://4.bp.blogspot.com/-REN2llx_jAo/Wx_6MDsmjVI/AAAAAAAAB7s/NmK3OwKsfbwPC2mhVpuYydvyqR4MNJrhACLcBGAs/s1600/listorderitem.png" imageanchor="1" ><img border="0" src="https://4.bp.blogspot.com/-REN2llx_jAo/Wx_6MDsmjVI/AAAAAAAAB7s/NmK3OwKsfbwPC2mhVpuYydvyqR4MNJrhACLcBGAs/s1600/listorderitem.png" data-original-width="1600" data-original-height="607" /></a>

#### Repository Link (Branch)

[Part IX](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_IX_Entity_Relationns_Many_To_Many)



