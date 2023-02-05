module Client.View.Timer

open Lit
open LitStore

let private hmr = HMR.createToken ()


[<HookComponent>]
let view (model:Client.Timer.Model) dispatch =
    Hook.useHmr (hmr)
   
    html
        $"""
        <div> {model.Time }</div>
        """
