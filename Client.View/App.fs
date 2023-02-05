module Client.View.App
open Elmish
open Elmish.HMR
open Lit
open Lit.Elmish
open Browser.Types
open Fable.Core.JsInterop
open Fable.Core
open System
open Browser
open Elmish.Debug

let private hmr = HMR.createToken ()


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
  
open Client.App

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
        { StartStop.view model.StartStopModel ( StartStopMsg >> dispatch)}
        { Timer.view model.TimerModel ( TimerMessage >> dispatch)}
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
#if DEBUG
        |> Program.withDebugger
#endif

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
  