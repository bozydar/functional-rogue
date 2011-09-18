module Predefined.Parser

open Board
open System.Drawing
open System
    

let def = [
    ('0', Place.EmptyPlace); 
    ('#', Place.Wall);
    ('g', { Place.Wall with Tile = Tile.Glass});
    ('>', { Place.EmptyPlace with Tile = Tile.StairsDown});
    ('+', { Place.EmptyPlace with Tile = Tile.ClosedDoor});
    ('.', { Place.EmptyPlace with Tile = Tile.Floor});
    (',', { Place.EmptyPlace with Tile = Tile.Grass})]

let (|Definition|_|) (def : (char * Place) list) input =
    match List.tryFind (fun item -> input = fst item ) def with
    | Some(_, place) -> Some(place)
    | _ -> Option.None

let parse defs (text : string) = 
    let lines =
        text.Split('\n')
        |> Array.toList 
        |> List.map (fun item -> item.Trim())
    let places = [
        for item in lines do
            yield [
                for char in item do 
                yield
                    match char with
                    | Definition defs place -> place
                    | _ -> failwith <| "Unknown char: " + char.ToString()
            ]                
    ]    
    places 

let putPredefinedOnBoard defs (position : Point) pattern board =    
    let parsed = parse defs pattern
    let modifiers = seq {        
        let pairs = Seq.mapi (fun i item -> position.Y + i, item) parsed
        for y, items in pairs do   
            let pairs2 = Seq.mapi (fun i item -> position.X + i, item) items
            for x, item in pairs2 do
                yield Board.set (new Point(x, y)) item

    }
    board
    |>> modifiers     
