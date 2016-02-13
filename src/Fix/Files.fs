module Files

open System
open System.IO
open Fake
open Common
open Fix.ProjectSystem

let nodeType fileName =
    match Path.GetExtension fileName with
    | ".fs" -> "Compile"
    | ".config" | ".html" -> "Content"
    | _ -> "None"

let getTemplates () =
    Definitions.Load(templateFile).Files |> Array.map (fun n -> n.Name, n.Value, n.Extension)

let Order file1 file2 =
    Project.execOnProject (fun x -> x.OrderFiles file1 file2)

let sed (find:string) replace file =
    let r = replace file
    let contents = File.ReadAllText(file).Replace(find, r)
    File.WriteAllText(file, contents)


let Add fileName template =
    let templates = getTemplates ()
    let template' = if String.IsNullOrWhiteSpace template then templates |> Array.map( fun (n,v,_) -> n,v) |> promptSelect2 "Choose a template:" else template
    let (_, value, ext) = templates |> Seq.find (fun (_,v,_) -> v = template')
    let oldFile = value + "." + ext
    let newFile = fileName + "." + ext
    let newFile' =  (directory </> newFile)

    Fake.FileHelper.CopyFile newFile' (filesLocation </> oldFile)
    let node = nodeType newFile
    Project.execOnProject (fun x -> x.AddFile newFile node)

    sed "<%= namespace %>" (fun _ -> fileName) newFile'
    sed "<%= guid %>" (fun _ -> System.Guid.NewGuid().ToString()) newFile'
    sed "<%= paketPath %>" (relative directory) newFile'
    sed "<%= packagesPath %>" (relative packagesDirectory) newFile'



let Remove fileName =
    Project.execOnProject (fun x -> x.RemoveFile fileName)
    directory </> fileName |> Fake.FileHelper.DeleteFile

let List () =
    let listFilesOfProject (project:ProjectFile) =
        project.ProjectFiles
        |> List.iter (printfn "%s")
        project

    Project.execOnProject listFilesOfProject
