[<AutoOpen>]
module Utils

open System
open Microsoft.FSharp.Reflection

let repr (x:'a) =
    let unionRepr (x:'a) =    
        match FSharpValue.GetUnionFields(x, typeof<'a>) with
        | case, _ -> case.Name

    if FSharpType.IsUnion(typeof<'a>) then unionRepr x
    else x.ToString()

let private r = new Random()
let rnd max = 
    r.Next(max)

let rnd2 min max = 
    r.Next(min, max)
