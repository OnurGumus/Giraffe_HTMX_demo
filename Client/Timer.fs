module Client.Timer
open Elmish
open Client.Timecontext
open System.Threading

type Model = { Time : int; State : Timecontext.State; CT : CancellationTokenSource  option }

type Msg = Tick | Start | Stop

let timer (token: CancellationToken)= 
    Cmd.ofEffect (fun dispatcher ->
        Async.StartImmediate (async { 
            while token.IsCancellationRequested |> not do
                do! Async.Sleep 1000
                dispatcher Tick
            } , token) 
        )

let f dispatcher (state : Timecontext.Model)  = 
    match state.State with 
    | Timecontext.Started -> dispatcher Msg.Start 
    | _ -> dispatcher Msg.Stop

let initCmd  = 
    Cmd.ofEffect (fun dispatcher -> Timecontext.store.Subscribe (f dispatcher) |> ignore)

let init ()  =
    { Time = 0 ; State = Stopped; CT = None }, initCmd

let update msg model =
    match msg with
    | Tick -> { model with Time = model.Time + 1 }, Cmd.none
    | Start -> 
        let cts = new CancellationTokenSource()
        { model with State = Started; CT = Some cts }, timer cts.Token
    | Stop -> 
        match model.CT with
        | Some ct -> 
            ct.Cancel()
            ct.Dispose()
        | _ -> ()
        { model with  State = Stopped; CT = None }, Cmd.none