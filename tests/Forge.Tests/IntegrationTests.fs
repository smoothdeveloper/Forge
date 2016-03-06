module IntegrationTests

open System.IO
open Forge.Prelude
open NUnit.Framework

let integrationTestCasesFolder = DirectoryInfo(__SOURCE_DIRECTORY__ </> ".." </> "integration.test.cases")

type CompareResult =
| SameFolder of (string * CompareResultItem) seq
| FolderDifferent of (string * CompareResultItem) seq
and CompareResultItem =
| SameFile
| LeftFileAbsent
| RightFileAbsent
| FileContentsDifference
| LeftSubFolderAbsent
| RightSubFolderAbsent


let getRelativePath (dir: DirectoryInfo) (fsInfo: FileSystemInfo) = fsInfo.FullName.Substring(dir.FullName.Length)

let compareFiles (f1: FileInfo) (f2: FileInfo) =
  if f1.Length <> f2.Length then
    FileContentsDifference
  else
    let c1 = File.ReadAllBytes(f1.FullName)
    let c2 = File.ReadAllBytes(f2.FullName)
    if c1 = c2 then
      SameFile
    else
      FileContentsDifference

let getFilesDict (d: DirectoryInfo) = d.GetFiles() |> Seq.map (fun f -> f.Name, f) |> dict
let getDirectoriesDict (d: DirectoryInfo) = d.GetDirectories() |> Seq.map (fun d -> d.Name, d) |> dict

let compareFolders (d1: DirectoryInfo) (d2: DirectoryInfo) =
  
  let rec compareFolders root d1 d2 =
    
    let files1 = getFilesDict d1
    let files2 = getFilesDict d2

    let dirs1 = getDirectoriesDict d1
    let dirs2 = getDirectoriesDict d2

    seq {
      for f1 in files1.Values do
        
        let relativePath = getRelativePath root f1
        
        match files2.TryGetValue(f1.Name) with
        | true, f2 -> yield relativePath, compareFiles f1 f2
        | _        -> yield relativePath, RightFileAbsent

      for f2 in files2.Values do
        
        let relativePath = getRelativePath root f2
        
        match files1.TryGetValue(f2.Name) with
        | true, _  -> () // already compared
        | false, _ -> yield relativePath, LeftFileAbsent
      
      for d1 in dirs1.Values do
        
        let relativePath = getRelativePath root d1
        
        match dirs2.TryGetValue(d1.Name) with
        | true, d2 -> yield! compareFolders root d1 d2
        | false, _ -> yield relativePath, RightSubFolderAbsent

      for d2 in dirs2.Values do
        
        let relativePath = getRelativePath root d1

        match dirs1.TryGetValue(d2.Name) with
        | true, _  -> ()
        | false, _ -> yield relativePath, LeftSubFolderAbsent

    }
  let result = compareFolders d1 d1 d2
  if Seq.forall (function | _,SameFile -> true | _ -> false) result then
    SameFolder result
  else
    FolderDifferent result

let runForgeCommands = () //todo

let [<Test>] ``run all integration test cases`` () =
  let failures =
    [
      for testCase in integrationTestCasesFolder.GetDirectories() do
        let before   = DirectoryInfo(testCase.FullName </> "before")
        let after    = DirectoryInfo(testCase.FullName </> "after")
        let expected = DirectoryInfo(testCase.FullName </> "expected")
  
        if before.Exists then
    
          if after.Exists then
            after.Delete(true)
          Fake.FileUtils.cp_r before.FullName after.FullName
    
          if expected.Exists then
            runForgeCommands

            let result = compareFolders after expected

            match result with
            | FolderDifferent result -> yield testCase.Name, result
            | _                      -> ()
    ]
  if not failures.IsEmpty then
    Assert.Fail(sprintf "%A" failures)