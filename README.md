# GraphQL with ASP.NET Core (Part- VII : Mutation)

We've been dealing with data fetching so far. But how do you cause side effects on the server-side data? Side effects can be anything ranging from a data insertion, patching, deletion or update. GraphQL mutation is just the thing you are looking for here.

Before we move forward, I would like to do a bit of housekeeping on the project. So, I've changed the name of the *HelloWordQuery* object graph type to *InventoryQuery*. Also, the *HelloWordSchema* is now replaced with *InventorySchema*. And I've removed the `hello` and `howdy` fields out of the root query object.

A mutation type also extends from `ObjectGraphType`. Following `createItem` field creates an item on the server side and returns it.

*InventoryMutation.cs*

```
public class InventoryMutation : ObjectGraphType
{
    public InventoryMutation(IDataStore dataStore)
    {         
        Field<ItemType>(
            "createItem",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<ItemInputType>> { Name = "item" }
            ),
            resolve: context =>
            {
                var item = context.GetArgument<Item>("item");
                return dataStore.AddItem(item);
            });
    }
}
```

Notice, we have a new `ItemInputType` as query argument. Previously, in [Part-V](http://fiyazhasan.me/graphql-with-asp-net-core-part-v-fields-arguments-variables/), we had an example where we worked with a scaler type argument. But for passing a complex type as arguments, we have to work differently. Hence, come a new type i.e `InputObjectType`. I've created a new `ItemInputType` and extend it from `InputObjectGraphType`,

*ItemInputType.cs*

```
public class ItemInputType : InputObjectGraphType
{
    public ItemInputType()
    {
        Name = "ItemInput";
        Field<NonNullGraphType<StringGraphType>>("barcode");
        Field<NonNullGraphType<StringGraphType>>("title");
        Field<NonNullGraphType<DecimalGraphType>>("sellingPrice");
    }
}
``` 

Following code is from `DataStore`; it uses the `ApplciationDbContext` to add a new item to the `DBSet<Item> Items` collection,

*DataStore.cs*

```
public async Task<Item> AddItem(Item item)
{
    var addedItem = await _applicationDbContext.Items.AddAsync(item);
    await _applicationDbContext.SaveChangesAsync();
    return addedItem.Entity;
}
```

Notice, we have returned the added entity back to the `createItem` field endpoint so that we can query nested fields of the newly added item. And this is also the preferred way.

> Just like in queries, if the mutation field returns an object type, you can ask for nested fields. This can be useful for fetching the new state of an object after an update. - [GraphQl Org.](http://graphql.org/learn/queries/#mutations)

Before we can run our application, a couple of DI registrations are needed for `ItemInputType` and `InventoryMutation`,

*Startup.cs*

```
services.AddScoped<ItemInputType>();
services.AddScoped<InventoryMutation>();
``` 

Last but not least is, you register your schema with the newly created mutation object as following,

*InventorySchema.cs*

```
public class InventorySchema : Schema
{
	public InventorySchema(InventoryQuery query, InventoryMutation mutation)
    {
		Query = query;
        Mutation = mutation;
    }
}
```

Now, you can run a mutation within the in-browser IDE with a syntax like following,

```
mutation {
  createItem(item: {title: "GPU", barcode: "112", sellingPrice: 100}) {
    title
    barcode
  }
}
```

It will add the item passed within the `item` argument and return the `title` and `barcode` of that newly added item,

<a href="https://2.bp.blogspot.com/-i1zadLQQpT0/WwQIk7ctVKI/AAAAAAAAB6Q/YrTQb0BTNTMguPl_sUNBvjgby_nqu7AagCLcBGAs/s1600/Screen%2BShot%2B2018-05-22%2Bat%2B6.07.40%2BPM.png" imageanchor="1" ><img border="0" src="https://2.bp.blogspot.com/-i1zadLQQpT0/WwQIk7ctVKI/AAAAAAAAB6Q/YrTQb0BTNTMguPl_sUNBvjgby_nqu7AagCLcBGAs/s1600/Screen%2BShot%2B2018-05-22%2Bat%2B6.07.40%2BPM.png" data-original-width="1600" data-original-height="393" /></a>

You can also use variables to pass in the item argument via the `Query Variables` window,

<a href="https://4.bp.blogspot.com/-02kh8B4TqHM/WwQKAcv8u-I/AAAAAAAAB6c/YPvD2DJAr3sUiSGho8_6J-b_yG-kX76lACLcBGAs/s1600/Screen%2BShot%2B2018-05-22%2Bat%2B6.15.20%2BPM.png" imageanchor="1" ><img border="0" src="https://4.bp.blogspot.com/-02kh8B4TqHM/WwQKAcv8u-I/AAAAAAAAB6c/YPvD2DJAr3sUiSGho8_6J-b_yG-kX76lACLcBGAs/s1600/Screen%2BShot%2B2018-05-22%2Bat%2B6.15.20%2BPM.png" data-original-width="1600" data-original-height="740" /></a>

#### Repository Link (Branch)

[Part VII](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_VII_Mutation)

#### Important Links

[GraphQL Mutaion](http://graphql.org/learn/queries/#mutations)

[GraphQL Input Types](http://graphql.org/learn/schema/#input-types)

