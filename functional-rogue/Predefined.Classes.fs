[<RequireQualifiedAccessAttribute>]
module Predefined.Classes

open Microsoft.FSharp.Reflection
open Player

type PlayerClass = 
    | Navigator 
    | Soldier 
    | Cheff 
    | Stalker 

let getClasses = 
    let unionCaseInfos = FSharpType.GetUnionCases(typeof<PlayerClass>)
    unionCaseInfos |> Array.map (fun item -> item.Name)

let buildCharacterByPlayerClass item =
    match item.ToString() with 
    | "Navigator" -> new Player("Some Navigator", 20, 10, 16, 15, 300)
    | "Soldier" -> new Player("Some Soldier",     20, 10, 21, 10, 300)
    | "Cheff" -> new Player("Some Cheff",         20, 10, 16, 10, 500)
    | "Stalker" -> new Player("Some Stalker",     20, 15, 16, 10, 300)
    | _ -> failwith "Unknown option"