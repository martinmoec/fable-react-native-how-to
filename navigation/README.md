# Building apps with F# and Fable with smooth looking navigation

Even though Elmish is great for building single page apps, it does not play too well with large apps and navigation. If you have ever tried combining Elmish with classic app-navigation, you've probably already ripped half your hair out. Pushing multiple new screens atop your current view while keeping your navigation history and state, quickly becomes way more complicated that it has too be while it looks quirky. 

Using [React Navigation](https://reactnavigation.org) you can add professional navigation handling which looks and feels like the common user expects from a modern app. By combining with React Hooks, like `useReducer`, we can still hold on to a similar MVU-design as we are used to with Elmish.

This example implements a similar counter app like the Elmish sample. The counter screen is navigable and can be pushed multiple times, with the doubled value of the current counter, while keeping history and screen state.

## Setup a new React Native project

Start a standard new React Native project with `react-native init <project name>`

## Setup the F# project
Create a folder to hold your F# project and add the files `App.fsproj`, `App.fs` and `Counter.fs`. In this example i will create a `src` folder in the project root directory which contains the files `App.fsproj` and `App.fs`.

## Add npm modules

In addition to the npm and NuGet setup from the initial how-to you will have to add the following npm-packages for React Navigation.

`npm install @react-navigation/native @react-navigation/stack @react-navigation/bottom-tabs react-native-gesture-handler react-native-reanimated react-native-safe-area-context react-native-screens @react-native-community/masked-view`

### `App.fsproj`
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Counter.fs" />
    <Compile Include="App.fs" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Fable.Core" Version="3.1.5" />
    <PackageReference Include="Fable.React" Version="7.0.1" />
    <PackageReference Include="Fable.React.Native" Version="2.6.1" />
    <PackageReference Include="Fable.ReactNative.Navigation" Version="0.0.4-alpha" />
  </ItemGroup>
</Project>
```

Notice that the Elmish references are removed and a reference to `Fable.ReactNative.Navigation` is added, this will provide us with Fable bindings for the React Navigation library.

### `App.fs`

This app implements a similar counter like the Elmish-based how-to. However, in this example we will start the app at a home page and navigate to the counter using React Navigation. You will need to register your app with the name given at `react-native init` and provide a initial render function. This will register your navigation stack and screens which will be dispatched by React Navigation when navigating through the app.

Functions rendering from React Navigation calls can have a signature of `unit -> ReactElement` or `INavigation<'T> -> unit` where `'T` is the type of data you expect to be pushed with the screen. The `homePage` function renders out initial home page, here we grab the navigation object as we want to use it to navigate further with it. We do not really care about the properties here.

```fsharp
module App

open Fable.ReactNative.Navigation

module R = Fable.ReactNative.Helpers
module P = Fable.ReactNative.Props
open Fable.ReactNative.Props

// render function for the home screen
// the nav argument is added since we will be using it for navigation
// let homePage () =   would also work if we did not need the navigaiton prop
let homePage (nav : Types.INavigation<_>) =
    R.view [
        P.ViewProperties.Style [
            P.FlexStyle.Flex 1.
            P.FlexStyle.JustifyContent JustifyContent.Center
            P.FlexStyle.AlignItems ItemAlignment.Center
        ]
    ] [
        R.text [] "This is the home screen"

        R.touchableOpacity [
            OnPress(fun _ ->
                // push a new instance of the counter screen to the stack
                nav.navigation.push "counter"
            )
        ] [
            R.text [
                P.TextProperties.Style [
                    P.FlexStyle.MarginTop (R.pct 5.)
                ]
            ] "Open counter screen"
        ]
    ]

// the initial render function which creates the navigaiton container, 
// stack navigator and two stack screens

// this function will initialize the available screens
// React Navigation will push these to the stack and call the handler function (homePage and Counter.counter)
let render () =
    navigationContainer [] [
        Stack.navigator [
            // tell React Navigation to open the home screen initially
            Stack.NavigatorProps.InitialRouteName "home"
        ] [
            Stack.screen "home" homePage [] []
            Stack.screen "counter" Counter.counter [
                // the counter screen expects a param with an initial counter value
                // default is nont
                Stack.ScreenProps.InitialParams ({Initial = None} : Counter.CounterProps)
            ] []
        ]
    ]

// Update app name (react-native init <app name>)
Helpers.registerApp "You app name" (render ())
```

## `Counter.fs`

We will implement the classic counter sample, as demonstrated in the Elmish-based how-to, using function components and hooks instead of Elmish. Notice how we push a new counter screen with the double of the current counter value.

```fsharp
module Counter

open Fable.ReactNative.Navigation

type CounterProps = {
    Initial : int option
}

type private Model = {
    // Store the navigation object in the model to be user for later
    Navigation  : Types.INavigation<CounterProps>
    Counter     : int
}
and private Message =
    | Increment
    | Decrement

let private init nav = {
    Navigation  = nav
    Counter     = 
        // check the props given with the navigation object for a
        // initial counter value
        match nav.route.``params``.Initial with
        | None -> 0
        | Some i -> i
}

// standard update, but without the Cmd
let private update model msg =
    match msg with
    | Increment -> {model with Counter = model.Counter + 1 }
    | Decrement -> {model with Counter = model.Counter - 1 }

// a promise that will sleep for 2 sec before dispatching Increment
let private delayedIncrement dispatch =
    promise {
        do! Promise.sleep 2000
        dispatch Increment
    }

module R = Fable.ReactNative.Helpers
module P = Fable.ReactNative.Props
open Fable.ReactNative.Props

let private buttonStyle : IStyle list = [
    P.Color "#fff"
    P.FlexStyle.MarginTop (R.pct 5.)
    P.FlexStyle.Padding (R.pct 2.)
    P.BorderWidth 1.
    P.BorderColor "#fff"
]

// standard view
let private view model dispatch =
    R.view [
        P.ViewProperties.Style [ 
            P.FlexStyle.Flex 1.0 
            P.FlexStyle.JustifyContent JustifyContent.Center
            P.FlexStyle.AlignItems ItemAlignment.Center
            P.BackgroundColor "#131313" 
        ]
    ] [
        
        // increment button
        R.touchableOpacity [
            OnPress(fun _ -> dispatch Increment)
        ] [
            R.text [P.TextProperties.Style buttonStyle] "Increment"
        ]

        //decrement button
        R.touchableOpacity [
            OnPress(fun _ -> dispatch Decrement)
        ] [
            R.text [P.TextProperties.Style buttonStyle ] "Decrement"
        ]

        // delayed increment button
        R.touchableOpacity [
            OnPress(fun _ ->
                // start promise
                delayedIncrement dispatch
                |> Promise.start
            )
        ] [
            R.text [P.TextProperties.Style buttonStyle] "Delayed Increment"
        ]

        // display current counter
        R.text [
            P.TextProperties.Style [
                P.Color "#ffffff"
                P.FontSize 30.
                P.TextAlign P.TextAlignment.Center
                P.FlexStyle.MarginTop (R.pct 4.)
            ]
        ] (string model.Counter)

        // button for pushing new counter screen
        R.touchableOpacity [
            OnPress (fun _ ->
                // the expected data type
                // push the current counter * 2 to the new screen
                let props = {Initial = Some (model.Counter * 2)}

                // helper function from Fable.ReactNative.Navigation
                // helps push new screens with data
                // pushed props to screen "counter" through nav obj from model
                pushWithData model.Navigation "counter" props
            )
        ] [
            R.text [
                P.TextProperties.Style buttonStyle
            ] "Open counter again with double current count"
        ]
    ]


// create a function component with a reducer hook
// the counter function receives the navigation object with props

let counter (navigation : Types.INavigation<CounterProps> ) =

    Fable.React.FunctionComponent.Of(fun (props : {| nav : Types.INavigation<CounterProps> |} ) ->
        let initialModel = init props.nav
        // create a reducer hook
        let model = Fable.React.HookBindings.Hooks.useReducer(update, initialModel)

        view model.current model.update
    ) {|nav = navigation |}
```

## Links
- [React Navigation](https://reactnavigation.org)
- [Fable.ReactNative.Navigaion](https://github.com/martinmoec/Fable.ReactNative.Navigation)
- [React Hooks with useReducer](https://reactjs.org/docs/hooks-reference.html#usereducer)