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

type Factor = 
    | Value of decimal
    | Percent of decimal
    with override this.ToString () : string = 
            match this with
            | Value(v) -> v.ToString()
            | Percent(v) -> v.ToString("p")
type Factor with
    member this.IsZero = 
        match this with
        | Value(v) when v = 0M -> true
        | Percent(v) when v = 0M -> true
        | _ -> false

let inBoundary (v: int) max min = Math.Max(Math.Min(v, min), max)

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

let swap (a: _[]) x y =
    let tmp = a.[x]
    a.[x] <- a.[y]
    a.[y] <- tmp

let shuffleArray a =
    Array.iteri (fun i _ -> swap a i (r.Next(i, Array.length a))) a

let (|Key|_|) (charOrConsoleKey : obj) (input : ConsoleKeyInfo) =
    match charOrConsoleKey with
    | :? char as thisChar when thisChar = input.KeyChar -> Some()
    | :? ConsoleKey as thisKey when thisKey = input.Key -> Some()
    | _ -> None

let (|Keys|_|) (charsOrConsoleKeys : list<obj>) (input : ConsoleKeyInfo) =
    if charsOrConsoleKeys |> List.exists (fun item -> match input with | Key item -> true | _ -> false) then Some() else None


module Map =    
    let tryGetItem key (map : Map<_,_>) =
        if map.ContainsKey key then Some(map.[key]) else None