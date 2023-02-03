module View

let inline private html (s: string) = s

open System
open System.IO

let scriptFiles =
    let assetsDir  = "WebRoot/dist/assets"
    if Directory.Exists assetsDir then
        Directory.GetFiles(assetsDir, "*.js", SearchOption.AllDirectories)
    else
        [||]

let path = 
    scriptFiles 
    |> Array.map(fun x ->x.Substring(x.LastIndexOf(Path.DirectorySeparatorChar) + 1))

let layout (isDev) (body: string) =
    let script = 
        if isDev || path.Length = 0 then
            html $"""
            <script type="module" src="/dist/@vite/client"></script>
            <script type="module" src="/dist/build/App.js"></script>
            """
        else
            let scripts = 
                path 
                    |> Array.map(fun path ->
                    html $"""
                    <script type="module" src="/dist/assets/{path}" ></script>
                    """) 
            String.Join("\r\n",scripts)

    html
        $"""
<!DOCTYPE html>
<html lang="en">
    <head>
        <title>Server</title>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <meta name="theme-color" content="#3367D6" />
        <!-- <meta name="color-scheme" content="light dark" /> -->
        <meta name="description" content="🤓" />
        <meta name="mobile-web-app-capable" content="yes" />
    </head>
    <body>
        <main>
              <my-test></my-test>
        {body}
        </main>
        <script type="module" src="htmx.js"></script>
        <script src="https://unpkg.com/htmx.org/dist/ext/json-enc.js" defer></script>
        <script defer src="https://unpkg.com/alpinejs@3.x.x/dist/cdn.min.js"></script>
        {script}
        
    </body>
</html>"""


