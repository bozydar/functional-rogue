module LevelGeneration

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

let generateTest: Board = 
    let mutable board = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Floor}
    let rooms = (generateRooms []) |> Seq.take 4 |> Seq.toList
    let rec addRooms rooms board =
        match rooms with
        | [] -> board
        | item::t -> addRooms t <| (item :> IModifier).Modify board 
    addRooms rooms board

let generateDungeonRooms sections sectionWidth sectionHeight sectionsHorizontal sectionsVertical = 
    let rooms : Tunnel [,] = Array2D.create sectionsHorizontal sectionsVertical (new Tunnel(new Rectangle( 0, 0, 2, 2)))
    let resultRooms = Array2D.mapi ( fun x y i -> new Tunnel(new Rectangle((x*sectionWidth) + 1 + x, (y*sectionHeight) + 1 + y, sectionWidth, sectionHeight))) rooms 
//    match sections with
//    | head :: tail -> 
//        let x, y = head
//        new Tunnel(new Rectangle((x*sectionWidth) + 1 + x, (y*sectionHeight) + 1 + y, sectionWidth, sectionHeight)) :: (generateDungeonRooms tail sectionWidth sectionHeight)
//    | [] -> []
    resultRooms

let rec generateDungeonConnections sections rooms sectionWidth sectionHeight =
    match sections with
    | head :: tail ->
        let x, y = head
        let currentRoom = [List.head<Tunnel> rooms]
        List.append (generateDungeonConnections (List.tail sections) (List.tail<Tunnel> rooms) sectionWidth sectionHeight) currentRoom
    | [] -> []

let generateDungeonTunnels sections sectionWidth sectionHeight sectionsHorizontal sectionsVertical = 
    let rooms = generateDungeonRooms sections sectionWidth sectionHeight sectionsHorizontal sectionsVertical
    let roomsList = rooms |> Seq.cast<Tunnel> |> Seq.fold ( fun l n -> n :: l) []
    generateDungeonConnections sections roomsList sectionWidth sectionHeight

let generateDungeon: Board = 
    let mutable board = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Wall}
    let sectionsHorizontal = 4
    let sectionsVertical = 3
    let sectionWidth = ((boardWidth - 1) / sectionsHorizontal) - 1
    let sectionHeight = ((boardHeight - 1) / sectionsVertical) - 1
    let sections = [for x in 0..(sectionsHorizontal - 1) do for y in 0..(sectionsVertical - 1) do yield (x, y)]

    let rec addRooms rooms board =
        match rooms with
        | [] -> board
        | item::t -> addRooms t <| (item :> IModifier).Modify board 
    addRooms (generateDungeonTunnels sections sectionWidth sectionHeight sectionsHorizontal sectionsVertical) board

let generateLevel levelType : Board = 
    match levelType with
    | LevelType.Test -> generateTest
    | LevelType.Dungeon -> generateDungeon
    | _ -> failwith "unknown level type"

