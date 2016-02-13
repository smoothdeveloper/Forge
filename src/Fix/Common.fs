module Common
open System
open System.IO
open Fake
open FSharp.Data

let (^) = (<|)

let exeLocation = System.Reflection.Assembly.GetEntryAssembly().Location |> Path.GetDirectoryName
let templatesLocation = exeLocation </> ".." </> "templates"
let filesLocation = templatesLocation </> ".files"
let templateFile = templatesLocation </> "templates.json"

let directory = System.Environment.CurrentDirectory
let packagesDirectory = directory </> "packages"

let paketLocation = exeLocation </> "Tools" </> "Paket"
let fakeLocation = exeLocation </> "Tools" </> "FAKE"
let fakeToolLocation = fakeLocation </> "tools"

type Definitions = JsonProvider<""" {"Templates": [ { "name": "Console Application", "value": "console" }], "Files": [{ "name": "F# Module", "value": "fs", "extension": "fs" }]}""">

let relative (path1 : string) (path2 : string) =
    let p1 = Uri(path1)
    let p2 = Uri(path2)
    Uri.UnescapeDataString(
        p2.MakeRelativeUri(p1)
          .ToString()
          .Replace('/', Path.DirectorySeparatorChar)
    )

let prompt text =
    printfn text
    Console.Write("> ")
    Console.ReadLine()

let promptSelect text list =
    printfn text
    list |> Array.iter (printfn " - %s")
    printfn ""
    Console.Write("> ")
    Console.ReadLine()

let promptSelect2 text list =
    printfn text
    list |> Array.iter (fun (n, v) -> printfn " - %s (%s)" n v)
    printfn ""
    Console.Write("> ")
    Console.ReadLine()

let run cmd args dir =
    if execProcess( fun info ->
        info.FileName <- cmd
        if not ^ String.IsNullOrWhiteSpace dir then
            info.WorkingDirectory <- dir
        info.Arguments <- args
    ) System.TimeSpan.MaxValue = false then
        traceError ^ sprintf "Error while running '%s' with args: %s" cmd args
