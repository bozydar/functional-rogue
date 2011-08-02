﻿module LevelGeneration

open System.Drawing
open Board

let minRoomSize = 3

   
let isOverlapping room rooms =    

    let isOverlapping2 (room1: Room) (room2: Room) =         
        room1.Rectangle.IntersectsWith room2.Rectangle

    List.exists (isOverlapping2 room) rooms

let rec generateRooms rooms =
    seq {                        
        let rec newRoom'() = 
            let newRoom = 
                let posX = rnd (boardWidth - minRoomSize)
                let posY = rnd (boardHeight - minRoomSize)
                let width = rnd2 minRoomSize (boardWidth - posX)
                let height = rnd2 minRoomSize (boardHeight - posY)
                new Room(new Rectangle(posX, posY, width, height))
            if isOverlapping newRoom rooms 
            then
                newRoom'()
            else
                newRoom
        let newRoom = newRoom'()
        yield newRoom
        yield! generateRooms (newRoom::rooms)
    }

let addGold board =
    // returns sequence of board modification functions
    let modifiers = seq {
        for i in 0..20 do
            let posX = rnd boardWidth
            let posY = rnd boardHeight
            let value = rnd 10
            yield (fun board -> 
                Board.modify (point posX posY) (fun place -> 
                    {place with Items = Gold(value) :: place.Items} ) board)
    }
    // apply all modification functions on board
    board |>> modifiers

let generateLevel: Board = 
    let mutable board = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Floor}
    let rooms = (generateRooms []) |> Seq.take 4 |> Seq.toList
    let rec addRooms rooms board =
        match rooms with
        | [] -> board
        | item::t -> addRooms t <| (item :> IModifier).Modify board 
    addRooms rooms board
    |> addGold

