# GraphQL with ASP.NET Core - A 10 Part Blog Series

This repository contains a series of posts with branch wise code on builing scalable GraphQL end-points with ASP.NET Core. Each branch has its own readme where you can find rach relevent post. You can also read the series from my blog, 

http://fiyazhasan.me/tag/graphql-dotnet/

Ask anything you want in the comment section of my blog.

# Running the application

* Download the zip or clone the project
* Make sure you have necessary dotnet core sdks installed (I'm using ASP.NET Core 2.1)
>  https://www.microsoft.com/net/download/windows
* Build and run the project
> dotnet build
> dotnet run

### Optional ( If you want a real database behind the scene )
* Make sure you have Postgres installed in your system
> https://postgresapp.com/
* Change the connection string in `appsettings.json` to target your local posgres database.
* From command line go to the root of the project and create a db migration script
> dotnet ef migrations add Initial -o Data/Migrations
* Apply migration in your database
> dotnet ef database update


# Branches

[Part I - Hello World](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_I_Hello_World)

[Part II - Middleware](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_II_Middleware)

[Part III - Dependency Injection](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_III_Dependency_Injection)

[Part IV - GraphiQL - An in-browser IDE](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_IV_GraphIQL)

[Part V - Fields, Arguments, Variables](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_V_Fields_Arguments_Variables)

[Part VI - Persist Data - Postgres with EF Core](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_VI_Persist_Data)

[Part VII - Mutation](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_VII_Mutation)

[Part VIII - Entity Relations - One to Many](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_VIII_Entity_Relations_One_To_Many)

[Part IX - Entity Relations - Many to Many](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_IX_Entity_Relationns_Many_To_Many)

[Part X - Data Loader](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_X_DataLoader)

# Video Tutorials

Coming Soon...

# Mentions

[Joe McBride](https://twitter.com/UICraftsman) - Thanks for providing an awesome community driven project 
> [graphql-dotnet](https://github.com/graphql-dotnet/graphql-dotnet)

[Jon Galloway](https://twitter.com/jongalloway) - Thanks for featuring posts on ASP.NET Community Standup


 
