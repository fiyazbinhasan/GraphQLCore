# GraphQL with ASP.NET Core (Part- IV : GraphiQL - An in-browser IDE)

[`GraphiQL`](https://github.com/graphql/graphiql) (spelled `graphical`) is an in-browser IDE for exploring GraphQL. I think it's a must-have tool for any server running behind GraphQL. With `GraphiQL` in place, you can easily give yourself or your team an in-depth insight of your API.

There are setups you have to do first. We need some packages installed. Create a `package.json` file and paste the following snippet,

    {
      "name": "GraphQLAPI",
      "version": "1.0.0",
      "main": "index.js",
      "author": "Fiyaz Hasan",
      "license": "MIT",
      "dependencies": {
        "graphiql": "^0.11.11",
        "graphql": "^0.13.2",
        "isomorphic-fetch": "^2.2.1",
        "react": "^16.3.1",
        "react-dom": "^16.2.0"
      },
      "devDependencies": {
        "babel-cli": "^6.26.0",
        "babel-loader": "^7.1.4",
        "babel-preset-env": "^1.6.1",
        "babel-preset-react": "^6.24.1",
        "css-loader": "^0.28.11",
        "extract-text-webpack-plugin": "^3.0.2",
        "ignore-loader": "^0.1.2",
        "style-loader": "^0.20.3",
        "webpack": "^3.11.0"
      }
    }

At this point, you can either use `yarn` or `npm` to install the packages.

    yarn install

Or

    npm install

Next, create a `ClientApp` folder and add the following two files with the code snippets,

##### app.js

```
import React from 'react';
import ReactDOM from 'react-dom';
import GraphiQL from 'graphiql';
import fetch from 'isomorphic-fetch';
import 'graphiql/graphiql.css';
import './app.css';

function graphQLFetcher(graphQLParams) {
  return fetch(window.location.origin + '/api/graphql', {
    method: 'post',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(graphQLParams)
  }).then(response => response.json());
}

ReactDOM.render(
  <GraphiQL fetcher={graphQLFetcher} />,
  document.getElementById('app')
);
```

##### app.css

```
html, body {
    height: 100%;
    margin: 0;
    overflow: hidden;
    width: 100%
}

#app {
    height: 100vh
}
```

GraphiQL is a client-side library which provides a `React` component i.e. `<GraphiQL/>`. It renders the whole graphical user interface of the IDE. The component has a `fetcher` attribute which can be attached to a function. The attached function returns an HTTP promise object and it is just the mimic of the `POST` requests that we have been making with `Insomnia`/`Postman`. All of these are done in the `app.js`.

Next up is the `index.html`, which will pop up once our application is served. We render the `<GraphiQL/>` component in a `div` with an id of `app`. The index file is placed under the `wwwroot` so that it is publicly available.

##### index.html

```
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width" />
    <title>GraphiQL</title>
    <link rel="stylesheet" href="/style.css" />
</head>
<body>
    <div id="app"></div>
    <script src="/bundle.js" type="text/javascript"></script>
</body>
</html>
```

As you can see, we have a `bundle.js` and `style.css` file being referenced. Both of them are products of running a build automation script. In our case, we have `webpack` and the script is as follows,

##### webpack.config.js

```
const webpack = require('webpack');
var path = require('path');
const ExtractTextPlugin = require('extract-text-webpack-plugin');

module.exports = [
    {
        entry: {
            'bundle': './ClientApp/app.js',
        },

        output: {
            path: path.resolve('./wwwroot'),
            filename: '[name].js'
        },

        resolve: {
            extensions: ['.js', '.json']
        },

        module: {
            rules: [
                { test: /\.js/, use: [{
                    loader: 'babel-loader'
                }], exclude: /node_modules/ },
                {
                    test: /\.css$/, use: ExtractTextPlugin.extract({
                        fallback: "style-loader",
                        use: "css-loader"
                    })
                },
                { test: /\.flow/, use: [{
                    loader: 'ignore-loader'
                }] }
            ]
        },

        plugins: [
            new ExtractTextPlugin('style.css', { allChunks: true })
        ]
    }
];
```

The configuration is pretty much self-explanatory. It takes all the `.js` files in the `ClintApp` folder, the dependencies from the `node_module` and compiles them into a single `bundle.js` file. Similarly, the user-defined and library style files are compiled into a single `style.css` file. Both of the compiled files are sent to the `wwwroot` to make them publicly available.

Last of all a `.babelrc` configuration file is needed to define the presets as followings,

##### .babelrc

```
{
  "presets": ["env", "react"]
}
```

All done! Now run the `webpack` command in the terminal on the root of your project and you will have the `bundle.js` and `style.css` files generated.

On the server-side, in `Startup.cs` files, add the middlewares to serve static files and espacially the default `index.html` file,

The `Configure` method should look like the following,

```
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseMiddleware<GraphQLMiddleware>();
}
```

Now, run the application and you will be presented with the following interface,

<a href="https://4.bp.blogspot.com/-jyUyY3Ug6rY/WtTnfuaS-jI/AAAAAAAAB2k/QiAhyrDpYFQnKALSGzlLXTuWnvcNNha8ACLcBGAs/s1600/GraphiQL.png" imageanchor="1" ><img border="0" src="https://4.bp.blogspot.com/-jyUyY3Ug6rY/WtTnfuaS-jI/AAAAAAAAB2k/QiAhyrDpYFQnKALSGzlLXTuWnvcNNha8ACLcBGAs/s1600/GraphiQL.png" data-original-width="1600" data-original-height="1044" /></a>

On the right-hand side documentation explorer pane, you can browse through different queries and have a deep understanding of what fields are available and what they supposed to do.

Some of the nice features this IDE has to offers are as followings,

* Syntax highlighting
* Intelligent type ahead of fields, arguments, types, and more.
* Real-time error highlighting and reporting.
* Automatic query completion.
* Run and inspect query results.

#### Repository Link (Branch)

[Part IV](https://github.com/fiyazbinhasan/GraphQLCore/tree/Part_III_Dependency_Injection)

#### Important Links

[Github repository for GraphiQL](https://github.com/graphql/graphiql)

[Concepts of webpack](https://webpack.js.org/concepts/)

[React.js Hello World](https://reactjs.org/docs/hello-world.html)

[Babel.js installation guide for webpack](https://babeljs.io/docs/setup/#installation)
