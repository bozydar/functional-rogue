[<AutoOpen>]
module Utils

open System
open System.Drawing
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

let point x y  = new Point(x, y)

let (|>>) v l = Seq.fold (|>) v l