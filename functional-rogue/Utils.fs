[<AutoOpen>]
module Utils

open Microsoft.FSharp.Reflection

let unionRepr (x:'a) =    
    match FSharpValue.GetUnionFields(x, typeof<'a>) with
    | case, _ -> case.Name

let repr (x:'a) =
    if FSharpType.IsUnion(typeof<'a>) then unionRepr x
    else x.ToString()
