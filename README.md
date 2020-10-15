# How-to: Set up React Native with F# and Fable

This is a step-by-step guide to setup a React Native project with F# and Fable. This lets you write React Native apps targeting iOS and Android almost completely through F#! Sample files for getting started can be found within this repository.

## Requirements
- React Native
- Watchman
- Node.js
- .NET Core >= 3.0
- yarn (or npm, but i use yarn here)

# Setup a new React Native project

This how-to will not go into detail on how to install React Native and how React Native works. When you have React Native ready you can create a new project by running `react-native init <project name>`

You should test that your basic React Native project compiles/runs before moving further.

`react-native run-ios` or `react-native run-android`

# Setup the F# project
Create a folder to hold your F# project and add a `.fsproj` file with a simple `.fs` file. In this example i will create a `src` folder in the project root directory which contains the files `App.fsproj` and `App.fs`.

## `App.fsproj`
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="App.fs" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Fable.Core" Version="3.1.5" />
    <PackageReference Include="Fable.Elmish" Version="3.1.0" />
    <PackageReference Include="Fable.Elmish.React" Version="3.0.1" />
    <PackageReference Include="Fable.React" Version="7.0.1" />
    <PackageReference Include="Fable.React.Native" Version="2.6.1" />
  </ItemGroup>
</Project>
```

## `App.fs`
```fsharp
module App

open Elmish
open Elmish.React
open Elmish.ReactNative
open Fable.ReactNative

// A very simple app which increments a number when you press a button

type Model = {
    Counter : int
}

type Message =
    | Increment 

let init () = {Counter = 0}, Cmd.none

let update msg model =
    match msg  with
    | Increment ->
        {model with Counter = model.Counter + 1}, Cmd.none 

module R = Fable.ReactNative.Helpers
module P = Fable.ReactNative.Props
open Fable.ReactNative.Props

let view model dispatch =
    R.view [
        P.ViewProperties.Style [ 
            P.FlexStyle.Flex 1.0 
            P.FlexStyle.JustifyContent JustifyContent.Center
            P.BackgroundColor "#131313" ]
    ] [
        
        
        R.text [
            P.TextProperties.Style [ P.Color "#ffffff" ]
        ] "Press me"
        |> R.touchableHighlightWithChild [
            P.TouchableHighlightProperties.Style [
                P.FlexStyle.Padding (R.dip 10.)
            ]
            P.TouchableHighlightProperties.UnderlayColor "#f6f6f6"
            OnPress (fun _ -> dispatch Increment) 
        ]

        R.text [
            P.TextProperties.Style [
                P.Color "#ffffff"
                P.FontSize 30.
                P.TextAlign P.TextAlignment.Center
            ]
        ] (string model.Counter)
    ]

Program.mkProgram init update view
|> Program.withConsoleTrace
|> Program.withReactNative "Your project name" // CHANGE ME
|> Program.run
```

IMPORTANT: Feed the name of your project in `Program.withReactNative` (the same you used for `react-native init` )

# Install Nuget packages

`cd src && dotnet restore`

# Install npm-packages

Install the [fable-splitter](https://www.npmjs.com/package/fable-splitter), [fable-compiler](https://www.npmjs.com/package/fable-compiler) and the [@babel/preset-env](https://www.npmjs.com/package/@babel/preset-env) npm-modules as dev-dependencies. See the documentation of these for further info.

`yarn add --dev fable-splitter fable-compiler @babel/preset-env` 

You will also need to install the [buffer](https://www.npmjs.com/package/buffer) npm-module, along with the [@react-native-community/netinfo](https://www.npmjs.com/package/@react-native-community/netinfo) module which is required by Fable.React.Native.

`yarn add buffer @react-native-community/netinfo`

You can now compile your F# project to Javascript by simply running `yarn fable-splitter src/App.fsproj -o out`
(Note the `-o` parameter specifying the output folder to dump the .js files) 

If you get a compilation error it is likely to be caused by your `babel.config.js` file, and i've experienced that the easiest way to get rid of this i simply by deleting the `babel.config.js` file altogether. You can also provide a configuration file as shown below. However, someone with a better Babel understanding than me could probably provide a better configuration/setup (suggestions welcomed).

You can provide the `fable-splitter` with a config file for simpler configuration. For example:
```javascript
module.exports = {
    entry: "src/App.fsproj",
    outDir: "out",
    babel: {
      presets: [["@babel/preset-env", { modules: "commonjs" }]],
      filename: "App.js",
      sourceMaps: false,
    },
    // The `onCompiled` hook (optional) is raised after each compilation
    onCompiled() {
        console.log("Compilation finished!")
    }
  }
```
`fable-splitter -c splitter.config.js`

Note the `entry` value should be the path to whatever you called your `.fsproj` file and the `out` value should be your output folder. This sample configuration will compile your F# project located in `./src/App.fsproj` into JavaScript dumped into the `./out` folder.

#### Tips:
```json
"build": "fable-splitter -c splitter.config.js --define RELEASE",
"debug": "fable-splitter -c splitter.config.js --define DEBUG",
"watch": "fable-splitter -c splitter.config.js -w --define DEBUG"
```
Add the above JSON to the `scripts` section of the `packages.json` file and simply call `yarn build` to compile. Run `yarn watch` in order to watch for changes and enable hot-reloading as you change your F# code.

# Importing the generated JavaScript
Now you can compile your F# code to JavaScript and dump it to a folder. In the sample `splitter.config.js` file we specified that the generated JavaScript is to be dumped to the `out` folder.

1. Delete your default `App.js` file in the root directory.
2. Update your `index.js` file:
    ```js
    /**
    * @format
    */

    import { AppRegistry } from 'react-native';
    import * as App from './out/App';
    import { name as appName } from './app.json';
    ```

# You're good to go! 
1. Compile F# to JavaScript and watch for changes
    - `yarn fable-splitter -c splitter.config.js -w --define DEBUG`
    - or `yarn watch` if you altered the `scripts` section of `packages.json`
2. Run app
    - `react-native run-ios|android`
3. Watch as the app updates along with your F# code. Enjoy!

