module Client.Timecontext

open ElmishStore
open Fable
open System

type State = Started | Stopped
type Model = 
    {
       State : State
    }

let init () = { State = Stopped }, Cmd.none

type Msg = 
    | Start
    | Stop

let update (msg: Msg) (model: Model) =
    match msg with
    | Start -> { model with State = Started }, Cmd.none
    | Stop -> { model with State = Stopped }, Cmd.none

let dispose _ = ()

let store, dispatch = Store.makeElmish init update dispose ()