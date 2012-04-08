[<RequireQualifiedAccessAttribute>]
module Predefined.Classes

open Microsoft.FSharp.Reflection

type PlayerClass = 
    | Navigator
    | Soldier
    | Medic
    | Engineer

let getClasses = 
    let unionCaseInfos = FSharpType.GetUnionCases(typeof<PlayerClass>)
    unionCaseInfos |> Array.map (fun item -> item.Name)

