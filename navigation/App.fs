module App

open Fable.ReactNative.Navigation

module R = Fable.ReactNative.Helpers
module P = Fable.ReactNative.Props
open Fable.ReactNative.Props

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

let render () =
    navigationContainer [] [
        Stack.navigator [
            Stack.NavigatorProps.InitialRouteName "home"
        ] [
            Stack.screen "home" homePage [] []
            Stack.screen "counter" Counter.counter [
                Stack.ScreenProps.InitialParams ({Initial = None} : Counter.CounterProps)
            ] []
        ]
    ]

// Update app name (react-native init <app name>)
Helpers.registerApp "You app name" (render ())