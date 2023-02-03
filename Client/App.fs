module App

open Elmish
open Elmish.HMR
open Lit
open Lit.Elmish
open Components
open Browser.Types

open Browser

let private hmr = HMR.createToken ()

type Cell = { Name: string; Value: decimal option }
type Key = Key of string
type RowData =
    { Key: Key
      Row: Cell list }

type Model = { Rows : RowData list; Counter : int}

type Message =
    | AddRow
    | RemoveRow of Key

let columnNames = [ "Length"; "Width"; "Height"; "Weight"; "Quantity" ]

let init () =
    { Rows = [{Key = Key "r0" ; Row = columnNames |> List.map (fun name -> { Name = name; Value = None }) }]; Counter = 1}, Cmd.none

let update msg model =
    match msg with
    | AddRow -> 
        let key = "r" + model.Counter.ToString()
        let row = {Key = Key key ; Row = columnNames |> List.map (fun name -> { Name = name; Value = None })  }
        { model with Rows = model.Rows @ [ row ]; Counter = model.Counter + 1 }, Cmd.none
    | RemoveRow key -> 
        { model with Rows = model.Rows |> List.filter (fun row -> row.Key <> key) }, Cmd.none

open Fable.Core.JsInterop
open Fable.Core
open System
type LitBindings =
    [<ImportMember("lit/directives/live.js")>]
    static member live(obj:obj) : unit = jsNative
    [<ImportMember("@lit-labs/motion")>]
    static member animate([<ParamArray>] args: obj[]) : unit = jsNative

[<Import("fadeIn", from="@lit-labs/motion")>]
let fadeIn : obj= jsNative

[<Import("fadeOut", from="@lit-labs/motion")>]
let fadeOut : obj= jsNative

[<Import("AnimateController", from="@lit-labs/motion")>]
type AnimateController (el: LitElement, obj) =
    member val onComplete: unit -> unit= jsNative with get,set

type MouseController (host:LitElement) as this=
    do host?addController(this)
    [<DefaultValue>] val mutable pos : float * float

    let onMouseMove = 
        fun (event :Event) -> 
            let event = event :?> MouseEvent
            this.pos<- (event.clientX,event.clientY)
            host.requestUpdate();
    let hostConnected = window.addEventListener("mousemove", onMouseMove);
  


let view (host:LitElement) (model:Model) dispatch =
    let renderCell (Key key) (cell: Cell) =
        html
            $"""
                <td>
                    <input required  name="{key}_{cell.Name}" 
                        .value={cell.Value |> Option.map string |> Option.defaultValue ""  } >
                </td>
            """
    let renderRow removeButton lastRow row  = 
        let cells = row.Row |> List.map(fun  cell -> renderCell row.Key cell )
        let removeButton = 
            if removeButton then 
                html $"""<td><button @click={Ev(fun ev -> dispatch (RemoveRow row.Key) )} type="button">Remove</button></td>""" 
            else 
                html $"<td></td>"

        let addButton = 
            if lastRow then 
                html $"""<td><button @click={Ev(fun ev -> dispatch AddRow)} type="button">Add</button></td>""" 
            else html $"<td></td>"

        let cells = cells @ [ removeButton ; addButton ]
        html $"""<tr  { LitBindings.animate({|  keyframeOptions={| duration = 1000;  |}; ``in`` = fadeIn; ``out``=fadeOut|})}> { cells } </tr>"""
    let rows = 
        model.Rows |>  Lit.mapiUnique (fun { Key = Key key} -> key) (fun i -> renderRow (model.Rows.Length > 1) (i = model.Rows.Length - 1))
  
    let onSubmit (ev:Event) = 
        ev.preventDefault()
        let formData =  FormData.Create(!!ev.target)
        let data = formData.entries()|> Seq.map(fun (key, value) -> key, value) |> Map.ofSeq
        printf "%A" data
    let mouse:MouseController = !!host?mouse
    html
        $"""
        <form @submit={Ev(onSubmit)}>
            <table>
                <tr>
                    <th>Length</th>
                    <th>Width</th>
                    <th>Height</th>
                    <th>Weight</th>
                    <th>Quantity</th>
                    <th></th>
                    <th></th>
                </tr>
                { rows }
            </table>
            <input  class="{Lit.classes["slid"]}" type="submit" { LitBindings.animate({|  keyframeOptions={| duration = 1000;  |}; ``in`` = fadeIn; ``out``= fadeOut|})} value="Submit">
        </form>
    """

[<LitElement("my-test")>]
let App () =
    let style = css $"""
       
        .slid {{
            height: 100px;
        }}
        """
    let prog = 
        Program.mkHidden init update

    Hook.useHmr (hmr)
    let host , p = LitElement.init (fun config -> config.useShadowDom <- true; config.styles <- [style]) 
    // Hook.useEffectOnce (fun () -> 
    //     let anim = new AnimateController(host,{|defaultOptions = {|  |}|}  )
    //     anim.onComplete <- (fun () -> printf "Animation complete")
    //     host?animController <- anim
    //     host?mouse <- new MouseController(host);
    // )

    
    let model, dispatch = Hook.useElmish (prog)
    view host model (dispatch)
  