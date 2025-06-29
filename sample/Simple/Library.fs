module Simple

open System
open Fable.Core
open Fable.Core.JsInterop
open MarkdownIt

let mdDefault = markdownit.Invoke()
let mdCommonmark = markdownit.Invoke(MarkdownIt.PresetName.Commonmark)
let result = mdDefault.render ("# markdown-it rulezz!")
printfn "%s" result
let result2 = mdCommonmark.render ("# markdown-it rulezz!")
printfn "%s" result2

// full options list (defaults)
let md =
    markdownit.Invoke(
        MarkdownIt.Options(
            // Enable HTML tags in source
            html = false,

            // Use '/' to close single tags (<br />).
            // This is only for full CommonMark compatibility.
            xhtmlOut = false,

            // Convert '\n' in paragraphs into <br>
            breaks = false,

            // CSS language prefix for fenced blocks. Can be
            // useful for external highlighters.
            langPrefix = "language-",

            // Autoconvert URL-like text to links
            linkify = false,

            // Enable some language-neutral replacement + quotes beautification
            // For the full list of replacements, see https://github.com/markdown-it/markdown-it/blob/master/lib/rules_core/replacements.mjs
            typographer = false,

            // Double + single quotes replacement pairs, when typographer enabled,
            // and smartquotes on. Could be either a String or an Array.
            //
            // For example, you can use '«»„“' for Russian, '„“‚‘' for German,
            // and ['«\xA0', '\xA0»', '‹\xA0', '\xA0›'] for French (including nbsp).
            quotes = !^"“”‘’",

            // Highlighter function. Should return escaped HTML,
            // or '' if the source string is not changed and should be escaped externally.
            // If result starts with <pre... internal wrapper is skipped.
            highlight = fun str lang attrs -> ""
        )
    )

let result3 = md.render ("# markdown-it rulezz!")
printfn "%s" result3

let plugin1 =
    MarkdownIt.PluginSimple(fun md ->
        // Example plugin that does nothing
        // You can add your own logic here
        ()
    )

let plugin2 =
    MarkdownIt.PluginWithParams(fun md args ->
        // Example plugin that does nothing with parameters
        // You can add your own logic here
        printfn "Plugin with params called with %A" args
    )

let plugin3 =
    MarkdownIt.PluginWithOptions(fun md options ->
        // Example plugin that does nothing with options
        // You can add your own logic here
        match options with
        | Some opts -> printfn "Plugin with options called with %A" opts
        | None -> printfn "Plugin with options called without options"
    )

let mdPlugins =
    markdownit.Invoke().``use``(plugin1).``use``(plugin2, 1, "2").``use`` (plugin3)

let result4 = mdPlugins.render ("# markdown-it rulezz!")
printfn "%s" result4
