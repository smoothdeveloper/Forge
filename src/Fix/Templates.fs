module Templates
open Common
open Fake
open Fake.Git
open System.IO
open FSharp.Data


let Refresh () =
    printfn "Getting templates..."
    templatesLocation|> FileHelper.CleanDir
    Repository.cloneSingleBranch (exeLocation </> "..") "https://github.com/fsprojects/generator-fsharp.git" "templates" "templates"


let GetList () =
     
    Definitions.Load(templateFile).Templates |> Array.map (fun t -> t.Name, t.Value)
