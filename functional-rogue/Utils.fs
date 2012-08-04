namespace FunctionalRogue

[<AutoOpen>]
module Utils =

    open System
    open System.Drawing
    open Microsoft.FSharp.Reflection

    let getUnionCaseName (x:'a) = 
        match FSharpValue.GetUnionFields(x, typeof<'a>) with
        | case, _ -> case.Name 

    let repr (x:'a) =
        if FSharpType.IsUnion(typeof<'a>) then getUnionCaseName x
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

    let inBoundary v min max = 
        if min > max then failwith "Min should be le max"
        elif v < min then min
        elif v > max then max
        else v    

    let isInBoundary v min max = 
        v >= min && v <= max

    let intByIndex tuple index = 
        FSharpValue.GetTupleField(tuple, index)  :?> int

    let self x = x

    let pointsDistance (point1: Point) (point2: Point) =
        max (abs(point1.X - point2.X)) (abs(point1.Y - point2.Y))

    (*
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
    *)

    let swap (a: _[]) x y =
        let tmp = a.[x]
        a.[x] <- a.[y]
        a.[y] <- tmp

    let shuffleArray a =
        Array.iteri (fun i _ -> swap a i (r.Next(i, Array.length a))) a

    let getBresenhamLinePoints (fromPoint : Point) (toPoint : Point) =
        let dx = Math.Abs(toPoint.X - fromPoint.X)
        let dy = Math.Abs(toPoint.Y - fromPoint.Y)
        let sx = if fromPoint.X < toPoint.X then 1 else -1
        let sy = if fromPoint.Y < toPoint.Y then 1 else -1
        let err = dx - dy
        let rec loop (currentPoint : Point) toPoint dx dy sx sy err =
            seq {
                if currentPoint = toPoint then
                    yield currentPoint
                else
                    yield currentPoint
                    let e2 = 2 * err
                    let nextCurrentX = if e2 > -dy then (currentPoint.X + sx) else currentPoint.X
                    let nextCurrentY = if e2 < dx then currentPoint.Y + sy else currentPoint.Y
                    let nextErr =
                        match e2 with
                        | value when value > -dy && value < dx -> err - dy + dx
                        | value when value > -dy -> err - dy
                        | value when value < dx -> err + dx
                        | _ -> err
                    yield! loop (Point(nextCurrentX, nextCurrentY)) toPoint dx dy sx sy nextErr
            }
        loop fromPoint toPoint dx dy sx sy err

    module Map =    
        let tryGetItem key (map : Map<_,_>) =
            if map.ContainsKey key then Some(map.[key]) else None
    module List =
        let removeAt index input =
            input 
            // Associate each element with a boolean flag specifying whether 
            // we want to keep the element in the resulting list
            |> List.mapi (fun i el -> (i <> index, el)) 
            // Remove elements for which the flag is 'false' and drop the flags
            |> List.filter fst |> List.map snd
        let insertAt index newEl input =
            // For each element, we generate a list of elements that should
            // replace the original one - either singleton list or two elements
            // for the specified index
            input 
            |> List.mapi (fun i el -> if i = index then [newEl; el] else [el])
            |> List.concat
