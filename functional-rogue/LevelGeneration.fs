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
    let rooms : Tunnel [,] = Array2D.create sectionsHorizontal sectionsVertical (new Tunnel(new Rectangle( 0, 0, 2, 2), true))
    let resultRooms = Array2D.mapi ( fun x y i -> new Tunnel(new Rectangle((x*sectionWidth) + 1 + x, (y*sectionHeight) + 1 + y, sectionWidth, sectionHeight), true)) rooms
    resultRooms

let generateTunnelConnection x1 y1 x2 y2 =
    let horizontalPart = [new Tunnel(new Rectangle(min x1 x2, min y1 y2, abs (x2 - x1) + 1, 1), false)]
    let varticalPart = [new Tunnel(new Rectangle(max x1 x2, min y1 y2, 1, abs (y2 - y1) + 1), false)]
    List.append horizontalPart varticalPart

let generateDungeonConnections sections (rooms: Tunnel[,]) sectionWidth sectionHeight sectionsHorizontal sectionsVertical =
    let mutable connectionList = []
    for x = 0 to sectionsHorizontal - 2 do 
            for y = 0 to sectionsVertical - 2 do
                if(x < (sectionsHorizontal - 1)) then
                    let horx1, hory1 = rooms.[x, y].GetRandomPointInside
                    let horx2, hory2 = rooms.[x + 1, y].GetRandomPointInside
                    connectionList <- List.append connectionList (generateTunnelConnection horx1 hory1 horx2 hory2)
                if(y < (sectionsVertical - 1)) then
                    let vertx1, verty1 = rooms.[x, y].GetRandomPointInside
                    let vertx2, verty2 = rooms.[x, y + 1].GetRandomPointInside
                    connectionList <- List.append connectionList (generateTunnelConnection vertx1 verty1 vertx2 verty2)
    connectionList

let generateDungeonTunnels sections sectionWidth sectionHeight sectionsHorizontal sectionsVertical = 
    let rooms = generateDungeonRooms sections sectionWidth sectionHeight sectionsHorizontal sectionsVertical
    let roomsList = rooms |> Seq.cast<Tunnel> |> Seq.fold ( fun l n -> n :: l) []
    let roomsWithConnectionsList = List.append roomsList (generateDungeonConnections sections rooms sectionWidth sectionHeight sectionsHorizontal sectionsVertical)
    roomsWithConnectionsList

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

