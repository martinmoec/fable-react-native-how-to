module Counter

open Fable.ReactNative.Navigation

type CounterProps = {
    Initial : int option
}

type private Model = {
    Navigation  : Types.INavigation<CounterProps>
    Counter     : int
}
and private Message =
    | Increment
    | Decrement

let private init nav = {
    Navigation  = nav
    Counter     = 
        match nav.route.``params``.Initial with
        | None -> 0
        | Some i -> i
}

let private update model msg =
    match msg with
    | Increment -> {model with Counter = model.Counter + 1 }
    | Decrement -> {model with Counter = model.Counter - 1 }

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

let private view model dispatch =
    R.view [
        P.ViewProperties.Style [ 
            P.FlexStyle.Flex 1.0 
            P.FlexStyle.JustifyContent JustifyContent.Center
            P.FlexStyle.AlignItems ItemAlignment.Center
            P.BackgroundColor "#131313" 
        ]
    ] [
        
        R.touchableOpacity [
            OnPress(fun _ -> dispatch Increment)
        ] [
            R.text [P.TextProperties.Style buttonStyle] "Increment"
        ]

        R.touchableOpacity [
            OnPress(fun _ -> dispatch Decrement)
        ] [
            R.text [P.TextProperties.Style buttonStyle ] "Decrement"
        ]

        R.touchableOpacity [
            OnPress(fun _ -> 
                delayedIncrement dispatch
                |> Promise.start
            )
        ] [
            R.text [P.TextProperties.Style buttonStyle] "Delayed Increment"
        ]

        R.text [
            P.TextProperties.Style [
                P.Color "#ffffff"
                P.FontSize 30.
                P.TextAlign P.TextAlignment.Center
                P.FlexStyle.MarginTop (R.pct 4.)
            ]
        ] (string model.Counter)

        R.touchableOpacity [
            OnPress (fun _ ->
                let props = {Initial = Some (model.Counter * 2)}
                pushWithData model.Navigation "counter" props
            )
        ] [
            R.text [
                P.TextProperties.Style buttonStyle
            ] "Open counter again with double current count"
        ]
    ]

let counter (navigation : Types.INavigation<CounterProps> ) =
    Fable.React.FunctionComponent.Of(fun (props : {| nav : Types.INavigation<CounterProps> |} ) ->
        let initialModel = init props.nav
        let model = Fable.React.HookBindings.Hooks.useReducer(update, initialModel)

        view model.current model.update
    ) {|nav = navigation |}