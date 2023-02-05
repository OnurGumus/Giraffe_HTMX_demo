module Client.App
open Elmish

type Cell = { Name: string; Value: decimal option }
type Key = Key of string
type RowData =
    { Key: Key
      Row: Cell list }

type Model = { Rows : RowData list; Counter : int; StartStopModel : StartStop.Model;  TimerModel : Timer.Model}

type Message =
    | AddRow
    | RemoveRow of Key
    | StartStopMsg of StartStop.Message
    | TimerMessage of Timer.Msg

let columnNames = [ "Length"; "Width"; "Height"; "Weight"; "Quantity" ]

let init () =
    let startStopModel, startStopCmd = StartStop.init()
    let timerModel, timerCmd = Timer.init()
    { 
        Rows = 
            [{Key = Key "r0"; Row = columnNames |> List.map (fun name -> { Name = name; Value = None }) }]
        Counter = 1
        StartStopModel = startStopModel
        TimerModel = timerModel
    }, Cmd.batch [Cmd.map StartStopMsg startStopCmd; Cmd.map TimerMessage timerCmd]


let update msg model =
    match msg with
    | AddRow -> 
        let key = "r" + model.Counter.ToString()
        let row = {Key = Key key ; Row = columnNames |> List.map (fun name -> { Name = name; Value = None })  }
        { model with Rows = model.Rows @ [ row ]; Counter = model.Counter + 1 }, Cmd.none
    | RemoveRow key -> 
        { model with Rows = model.Rows |> List.filter (fun row -> row.Key <> key) }, Cmd.none
    | StartStopMsg msg ->
        let startStopModel, startStopCmd = StartStop.update msg model.StartStopModel
        { model with StartStopModel = startStopModel }, Cmd.map StartStopMsg startStopCmd
    | TimerMessage msg ->
        let timerModel, timerCmd = Timer.update msg model.TimerModel
        { model with TimerModel = timerModel }, Cmd.map TimerMessage timerCmd