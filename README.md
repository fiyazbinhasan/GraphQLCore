# GraphQL with ASP.NET Core (Part- V : Fields, Arguments, Variables)

### Fields

We already have a good idea of GraphQL `Fields`. Remember we had two fields under the `HelloWorldQuery` i.e. `hello` and `howdy`. Both of them were scaler fields. As the official documentation states,

> "At its simplest, GraphQL is about asking for specific fields on objects" - [graphql.org](https://graphql.org/learn/queries/#fields)

Let's extend our simple application to accommodate a complex type. Say, for example, we are heading towards a path of making an `Inventory` system. Start by creating an `Item` type,

```
public class Item 
{
    public string Barcode { get; set; }

    public string Title { get; set; }

    public decimal SellingPrice { get; set; }
}
```

However, we can't directly query against this object as it is not a `GraphQL` object i.e. not an `ObjectGraphType`. To make it `GraphQL` queryable, we should create a new type and extend it from `ObjectGraphType`. Another flavor of `ObjectGraphType` takes generic types. As you already guessed it, we will pass the `Item` type as its generic argument.

```
public class ItemType : ObjectGraphType<Item>
{
    public ItemType()
    {
        Field(i => i.Barcode);
        Field(i => i.Title);
        Field(i => i.SellingPrice);
    }
}
```

Two things to notice down here. First, we no longer have type declarations in the fields. It will automatically get resolved by the library i.e. dot net `string` type will be resolved to `StringGraphType`. Second, we used `lambda` expressions to resolve things like the name of the `fields`. This concept of property matching should be easy to understand for the people who are familiar with the notion of `DTOs/ViewModels`. So, whoever thinks that we are dealing with an extra burden of type creation; trust me, we are not! 

Next, we need to register the `ItemType` in our root query object i.e. `HelloWorldQuery`,

```
public HelloWorldQuery()
{
    ...
    ...

    Field<ItemType>(
        "item",
        resolve: context =>
        {
           return new Item {
                Barcode = "123",
                Title = "Headphone",
                SellingPrice = 12.99M
            };
        }
    ); 
}
```

For the time being, we are returning a hard-coded instance of `Item` when someone tries to query the `item` field. We can run our application now and do the following,

<a href="https://3.bp.blogspot.com/-SyC7HeAZhLY/WuWhTJ14KOI/AAAAAAAAB3Y/h7vqDaOPSAwQtUJwSr0q0lCWGfhfTZ4DACLcBGAs/s1600/GraphiQL-complex.png" imageanchor="1" ><img border="0" src="https://3.bp.blogspot.com/-SyC7HeAZhLY/WuWhTJ14KOI/AAAAAAAAB3Y/h7vqDaOPSAwQtUJwSr0q0lCWGfhfTZ4DACLcBGAs/s1600/GraphiQL-complex.png" data-original-width="1600" data-original-height="1068" /></a>

### Arguments

Serving a hard-coded instance is not going to cut it. How about we introduce a data source which will serve the purpose of giving away a list of items,

```
public class DataSource
{
	public IList<Item> Items
	{
		get;
		set;
	}

	public DataSource()
	{
		Items = new List<Item>(){
			new Item { Barcode= "123", Title="Headphone", SellingPrice=50},
			new Item { Barcode= "456", Title="Keyboard", SellingPrice= 40},
			new Item { Barcode= "789", Title="Monitor", SellingPrice= 100}
		};
	}

	public Item GetItemByBarcode(string barcode)
	{
		return Items.First(i => i.Barcode.Equals(barcode));
	}
}
```

Along with the `Items` collection, we also have a method returns a single item that matches the a passed in barcode string.

Great! Now to pass in an argument via the query we have to modify the `item` field as followings,

 	Field<ItemType>(
		"item",
		arguments: new QueryArguments(new QueryArgument<StringGraphType> { Name = "barcode" }),
		resolve: context =>
		{
			var barcode = context.GetArgument<string>("barcode");
			return new DataSource().GetItemByBarcode(barcode);
		}
	);

There could be a list of arguments; some required and some optional. We specify an individual argument and its type with the `QueryArgument<T>`. The `Name` represents the name of the argument.

Now, we can construct a query inside `GraphiQL` with pre-defined arguments as followings,

```
query {
  item (barcode: "123") {
    title
    selling price
  }
}
```
At this point, the barcode argument is optional. So, if you do something like below,

```
query {
  item {
    title
    sellingPrice
  }
}
```

It will throw an error saying `Error trying to resolve item.`. That's fair since we didn't really write our code in a safe way. To ensure that user always provides an argument we can make the argument nonnullable with the following syntax,

```
QueryArgument<NonNullGraphType<StringGraphType>> { Name = "barcode" }
```

So, if we try to execute the same query in `GraphiQL`, it will give you nice error message as followings,


<a href="https://2.bp.blogspot.com/-VC6nHkPpd0w/WuWvMujlfLI/AAAAAAAAB3o/E-a30EOzvgUMr6EGKPrahGaVA6V6qzrTACLcBGAs/s1600/GraphiQL%2B%25281%2529.png" imageanchor="1" ><img border="0" src="https://2.bp.blogspot.com/-VC6nHkPpd0w/WuWvMujlfLI/AAAAAAAAB3o/E-a30EOzvgUMr6EGKPrahGaVA6V6qzrTACLcBGAs/s1600/GraphiQL%2B%25281%2529.png" data-original-width="1600" data-original-height="617" /></a>

### Variables

It's time to make the argument itself dynamic. We dont want to construct a whole query wherever we want to change a value of an argument, do we? Hence come variables. But first we have to make sure, our `GraphQL` middleware accepts variables. Go back to the `GraphQLRequest` class and add a `Variables` property.

    public class GraphQLRequest
    {
		public string Query { get; set; }
		public JObject Variables { get; set; }
    }

Next find out the `_executor.ExecuteAsync` method in the middleware's `InvokeAsync` method and modify as followings,

    var result = await _executor.ExecuteAsync(doc =>
    {
        doc.Schema = _schema;
        doc.Query = request.Query;

        doc.Inputs = request.Variables.ToInputs();

    }).ConfigureAwait(false);

Nice! our query is now ready to accept variables. Run the application and write a query as followings,

```
query($barcode: String!){
  item(barcode: $barcode){
    title
    sellingPrice
  }
}
```
Variable definitation starts with a `$` sign, followed by the variable type. Since we made the barcode argument non-nullable, here we also have to make sure the vaiable is non-nullable. Hence we used a `!` mark after the `String` type. 

To use the variable to send data we have a `Query Variables` pane inside `GraphiQL`. Use it to configure variables as followings,

<a href="https://3.bp.blogspot.com/-hDuiouT7Dsw/WuW1rSLrhyI/AAAAAAAAB34/s5PoKEiDRvo_6Z1OcJgjXB6Qhv8g9viwgCLcBGAs/s1600/GraphiQL%2B%25282%2529.png" imageanchor="1" ><img border="0" src="https://3.bp.blogspot.com/-hDuiouT7Dsw/WuW1rSLrhyI/AAAAAAAAB34/s5PoKEiDRvo_6Z1OcJgjXB6Qhv8g9viwgCLcBGAs/s1600/GraphiQL%2B%25282%2529.png" data-original-width="1600" data-original-height="712" /></a>

#### Repository Link (Branch)

[Part V](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_V_Fields_Arguments_Variables)

#### Important Links


[Create Data Transfer Objects (DTOs)](https://github.com/graphql/graphiql)

[Mapping Entity Framework Entities to DTOs with AutoMapper
](https://exceptionnotfound.net/entity-framework-and-wcf-mapping-entities-to-dtos-with-automapper/)

[GraphQL Fields](https://graphql.org/learn/queries/#fields)

[GraphQL Arguments](https://graphql.org/learn/queries/#arguments)

[GraphQL Variables](https://graphql.org/learn/queries/#variables)
