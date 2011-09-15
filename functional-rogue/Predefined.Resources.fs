module Predefined.Resources

open Parser
open Board
open System.Drawing
open System


let startLocationShip board =  
    let def = def @ [('1', { Place.Wall with Items = [Items.createPredefinedItem Items.OreExtractor]})]
    let where = Point(30, 10)
    board
    |> putPredefinedOnBoard def where
            "00##g#,,,,,,
             0g#1.#,,,,,,
             gg...+,,,,>0
             0g#..#,,,,,,
             00##g#,,,,,,"        
