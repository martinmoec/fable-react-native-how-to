# How-to: Set up React Native with F# and Fable

This is a step-by-step guide which aims to help you set up a React Native project with F# and Fable. The [Fable compiler](https://github.com/fable-compiler/Fable) generates JavaScript from your F# source code, and enables you to write React Native apps targeting iOS and Android almost completely through F#! Since the Fable compiler is just generating pure JavaScript from our F# source code we should be able to target any platform through React Native (Windows, MacOS, Web), though i have not tested this myself. Feel free to share your own experience with any of these platforms.

Sample files for getting started can be found within this repository. 

## Requirements
- React Native
- Watchman
- Node.js
- .NET Core >= 3.0
- npm

# Setup a new React Native project

This how-to will not go into detail on how to install React Native and how React Native works. When you have React Native installed you can create a new project by running `react-native init <project name>`

You should test that your basic React Native project compiles/runs before moving further.

`npx react-native run-ios` or `npx react-native run-android`

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

# Install Fable tool

`dotnet new tool-manifest && dotnet tool install fable`

# Install npm-packages

Install the [@babel/preset-env](https://www.npmjs.com/package/@babel/preset-env) npm-module as a dev-dependency. See the documentation of these for further info.

`npm install --save-dev @babel/preset-env` 

You will also need to install the [buffer](https://www.npmjs.com/package/buffer) npm-module, along with the [@react-native-community/netinfo](https://www.npmjs.com/package/@react-native-community/netinfo) module which is required by Fable.React.Native.

`npm install buffer @react-native-community/netinfo`

You can now compile your F# project to Javascript by simply running `dotnet fable ./src -o ./out`
(Note the `-o` parameter specifying the output folder to dump the .js files) 

If you get a compilation error it is likely to be caused by your `babel.config.js` file, and i've experienced that the easiest way to get rid of this i simply by deleting the `babel.config.js` file altogether. You can also provide a configuration file as shown below. However, someone with a better Babel understanding than me could probably provide a better configuration/setup (suggestions welcomed).

#### Tips:
```json
"build": "dotnet fable ./src -o ./out",
"watch": "dotnet fable watch ./src -o ./out"
```
Add the above JSON to the `scripts` section of the `packages.json` file and simply call `npm run build` to compile. Run `npm run watch` in order to watch for changes and enable hot-reloading as you change your F# code.

# Importing the generated JavaScript
Now you can compile your F# code to JavaScript and dump it to a folder (`./out` used in this example).

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
Notice that we import App from our generated files in the `out` folder. The app registration call is also removed, as this is now handled in our F# code.

# You're good to go! 
1. Compile F# to JavaScript and watch for changes
    - `dotnet fable watch ./src -o ./out`
    - or `npm run watch` if you altered the `scripts` section of `packages.json`
2. Run app
    - `npx react-native run-ios|android`
3. Watch as the app updates along with your F# code. Enjoy!

# More
- For larger apps you might want to opt out of Elmish and include navigation. Take a look at the following [how-to](navigation)