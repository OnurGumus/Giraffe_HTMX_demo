module Client.View.StartStop

open Lit
open LitStore

let private hmr = HMR.createToken ()


[<HookComponent>]
let view (model:Client.StartStop.Model) dispatch =
    Hook.useHmr (hmr)
    let handler _ = 
        match model.State with
        | Client.StartStop.Stopped -> dispatch Client.StartStop.Start
        | Client.StartStop.Started -> dispatch Client.StartStop.Stop
    html
        $"""
        <button @click={Ev handler}>{model.State }</button>
        """
