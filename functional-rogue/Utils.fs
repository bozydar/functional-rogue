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

type FloatingPoint = {
    X: float;
    Y: float
} with
    member this.ToPoint =
        let a = Convert.ToInt32 this.X
        let b = Convert.ToInt32 this.Y
        point a b
    static member (+) (left, right) =
        {X = left.X + right.X; Y = left.Y + right.Y}
    static member (+) (left, right: Point) =
        {X = left.X + Convert.ToDouble(right.X); Y = left.Y + Convert.ToDouble(right.Y)}

