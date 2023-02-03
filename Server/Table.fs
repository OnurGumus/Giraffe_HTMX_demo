module Table
open System.Text.Json
open System
open Giraffe
open Microsoft.AspNetCore.Http

let inline private html (s: string) = s

let columnNames = [ "Length"; "Width"; "Height"; "Weight"; "Quantity" ]

type Cell = { Name: string; Value: decimal option }


type RowData =
    { Key: string
      Row: Cell list
      IsLast: bool }

let renderTable (rowData: RowData list) =
    let renderRow (row: RowData) =
        let prefix = row.Key

        let renderCell (cell: Cell) =
            html
                $"""
                    <td>
                        <input  pattern="[-+]?[0-9]*[.,]?[0-9]+"  required name="{prefix}_{cell.Name}" 
                            value={cell.Value |> Option.map string |> Option.defaultValue ""} >
                    </td>
                """
        let renderAddButton =
            html
                $"""
                <td>
                    <button type="button"   hx-post="/table/addRow" >Add</button>
                </td>
            """

        let renderRemoveButton =
            if rowData.Length <= 1 then
                ""
            else
                html
                    $"""
                <td>
                    <button hx-post="/table/removeRow/{row.Key.Substring(1)}">Remove</button>
                </td>
            """

        let cells = row.Row |> List.map renderCell |> String.concat ""

        let buttons =
            if row.IsLast then
                renderRemoveButton + renderAddButton
            else
                renderRemoveButton
        html
            $"""
            <tr>
                {cells}
                {buttons}
            </tr>
        """
    let body =
        match rowData with
        | [] ->
            let rowData =
                { Key = "r1"
                  Row = columnNames |> List.map (fun x -> { Name = x; Value = None })
                  IsLast = true }

            renderRow rowData
        | _ -> rowData |> List.map (fun r -> renderRow r) |> String.concat ""

    html
        $"""
<form hx-ext='json-enc'  hx-target="this" hx-swap="outerHTML" x-data 
    @htmx:response-error="x=>alert(x.detail.xhr.responseText)" hx-validate >
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
    {body}
</table>
<input type="submit" value="Submit" />
</form>
"""

let numberParse =
    fun (s: string) ->
        match Decimal.TryParse s with
        | true, v -> Some v
        | false, _ -> None

let getRow body =
    task {
        let! jdoc = JsonDocument.ParseAsync(body)

        return
            jdoc.RootElement.EnumerateObject()
            |> Seq.map (fun x -> x.Name, x.Value.GetString())
            |> List.ofSeq
            |> List.groupBy (fun (name, _) -> name.Substring(0, name.IndexOf("_")))
            |> List.map (fun (key, items) ->
                { Key = key
                  Row =
                    items
                    |> List.map (fun (n, v) ->
                        { Name = n.Substring(n.IndexOf("_") + 1)
                          Value = v |> numberParse })
                  IsLast = false })
    }
let deleteRow =
    fun (id: int) (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! rowData = getRow ctx.Request.Body

            let rowData =
                rowData
                |> List.filter (fun x -> x.Key.Substring(1) <> string id)
                |> List.mapi (fun i r -> { r with IsLast = i = rowData.Length - 2 })

            let result = renderTable rowData
            return! htmlString result next ctx
        } 

let addRow =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! rowData = getRow ctx.Request.Body

            let maxKey = rowData |> List.map (fun k -> k.Key.Substring(1) |> int) |> List.max

            let rowData =
                rowData
                @ [ { Key = "r" + string (maxKey + 1)
                      Row = columnNames |> List.map (fun x -> { Name = x; Value = None })
                      IsLast = true } ]

            printf "%A" rowData
            let result = renderTable rowData
            return! htmlString result next ctx
        }