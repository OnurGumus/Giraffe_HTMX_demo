module Client.StartStop
open Elmish

type State = Started | Stopped

type Model = { State : State}

type Message = Start | Stop

let init () = 
    { State = Stopped }, Cmd.none

let update msg model =
    printf "%A" msg
    match msg with
    | Start -> { model with State = Started }, Cmd.ofEffect(fun _ -> Timecontext.dispatch Timecontext.Start)
    | Stop -> { model with State = Stopped }, Cmd.ofEffect(fun _ -> Timecontext.dispatch Timecontext.Stop)