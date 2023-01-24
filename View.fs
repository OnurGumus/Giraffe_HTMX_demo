module View

let inline private html (s: string) = s


let layout (isDev) (body: string) =
    let script =  ""

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
        {body}
        </main>
        <script type="module" src="htmx.js"></script>
        <script src="https://unpkg.com/htmx.org/dist/ext/json-enc.js" defer></script>
        <script defer src="https://unpkg.com/alpinejs@3.x.x/dist/cdn.min.js"></script>
        {script}
        
    </body>
</html>"""


