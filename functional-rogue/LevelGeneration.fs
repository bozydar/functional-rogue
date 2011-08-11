module LevelGeneration

open System.Drawing
open Board
open Items
open Monsters


// level generation utilities

let scatterTilesRamdomlyOnBoard (board: Board) (tileToPut:Tile) (backgroundTile:Tile) (probability:float) (createBorder:bool) : Board =
    let background = {Place.EmptyPlace with Tile = backgroundTile}
    let placeToPut = {Place.EmptyPlace with Tile = tileToPut}
    Array2D.mapi (fun x y i ->
        if(not(i.Tile = backgroundTile)) then
            i
        elif(createBorder && (x = 0 || x = (boardWidth - 1) || y = 0 || y = (boardHeight - 1))) then
            placeToPut
        else
            if((float)(rnd 100) < (probability * (float)100)) then placeToPut else i
           ) board

let countTileNeighbours (board: Board) x y (tileType:Tile)=
    let tileToSearch = {Place.EmptyPlace with Tile = tileType}
    let mutable count = 0
    for tmpx in x - 1 .. x + 1 do
        for tmpy in y - 1 .. y + 1 do
            if not(tmpx = x && tmpy = y) then
                count <- count + (if(board.[tmpx,tmpy] = tileToSearch) then 1 else 0)
    count

let rec smoothOutTheLevel board howManyTimes (tileToGrow:Tile) (backgroundTile:Tile) (rule:(int*int)) =
    let background = {Place.EmptyPlace with Tile = Tile.Floor}
    let toGrow = {Place.EmptyPlace with Tile = tileToGrow}
    let min, max = rule
    match howManyTimes with
    | 0 -> board
    | _ -> smoothOutTheLevel 
            (Array2D.mapi (fun x y i -> 
            if(x > 0 && x < (boardWidth - 1) && y > 0 && y < (boardHeight - 1)) then
                if((countTileNeighbours board x y tileToGrow) < min ) then background
                elif ((countTileNeighbours board x y tileToGrow) > max) then toGrow
                else i
            else i
                ) board)
            (howManyTimes - 1) tileToGrow backgroundTile rule


type DisjointLocationSet (board: Board, basicTile:Tile) =
     let mutable flags = Array2D.create (Array2D.length1 board) (Array2D.length2 board) -1
     let mutable current = 0

     let createSets =
        let rec floodLocation (board: Board) (flags: int[,]) x y current =
            let mutable tmpFlags = flags
            if(board.[x,y].Tile = basicTile && tmpFlags.[x,y] = -1) then
                Array2D.set tmpFlags x y current
                for floodx in (max (x - 1) 0) .. (min (x + 1) ((Array2D.length1 board) - 1)) do
                    for floody in (max (y - 1) 0) .. (min (y + 1) ((Array2D.length2 board) - 1)) do
                        tmpFlags <- floodLocation board tmpFlags floodx floody current
            tmpFlags

        for x in 0 .. (Array2D.length1 board) - 1 do
            for y in 0 .. (Array2D.length2 board) - 1 do
                if(board.[x,y].Tile = basicTile && flags.[x,y] = -1) then
                    current <- current + 1
                    flags <- (floodLocation board flags x y current)

     let mutable setSizes: int[] = Array.create current 0
     let mutable randomSetPoint = Array.create current (0,0)
     
     let countSizes =
        for x in 0 .. (Array2D.length1 board) - 1 do
            for y in 0 .. (Array2D.length2 board) - 1 do
                if(flags.[x,y] > 0) then
                    setSizes.[flags.[x,y] - 1] <- (setSizes.[flags.[x,y] - 1] + 1)
                    randomSetPoint.[flags.[x,y] - 1] <- (x,y)
     
     member this.NumberOfSections
        with get() = current

     member this.ConnectUnconnected : Board =
            let rec searchForMainSectionPiont (board: Board) x y i depth =
                match depth with
                | 0 -> if (flags.[x,y] = i) then (x,y) else (-1,-1)
                | _ ->
                    let mutable result = (-1,-1)
                    for searchx in (max (x - 1) 0) .. (min (x + 1) ((Array2D.length1 board) - 1)) do
                        for searchy in (max (y - 1) 0) .. (min (y + 1) ((Array2D.length2 board) - 1)) do
                            if (not(searchx = x && searchy = y)) then
                                let tmpResult = searchForMainSectionPiont board searchx searchy i (depth - 1)
                                if (tmpResult <> (-1,-1)) then
                                    result <- tmpResult
                    result

            let rec searchForClosestMainSectionPoint (board: Board) x y i trial=
                let mutable result = (-1,-1)
                result <- searchForMainSectionPiont board x y i trial
                if (result = (-1,-1)) then
                    result <- searchForClosestMainSectionPoint board x y i (trial + 1)
                result

            let rec digTunnel board x y targetx targety i =
                let mutable tmpBoard = board
                if(flags.[x,y] <> i) then
                    let basicPlace = {Place.EmptyPlace with Tile = basicTile}
                    let dx = x - targetx
                    let dy = y - targety
                    if((abs dx) > (abs dy)) then
                        let newx = (if(x > targetx) then (x - 1) else (x + 1))
                        Array2D.set board newx y basicPlace
                        tmpBoard <- digTunnel board newx y targetx targety i
                    else
                        let newy = (if(y > targety) then (y - 1) else (y + 1))
                        Array2D.set board x newy basicPlace
                        tmpBoard <- digTunnel board x newy targetx targety i
                tmpBoard

            let mutable tmpBoard = board
            let mutable theLargestSectionSize = 0
            let mutable theLargestSectionIndex = -1
            for i in 0 .. setSizes.Length - 1 do
                if(setSizes.[i] > theLargestSectionSize) then
                    theLargestSectionSize <- setSizes.[i]
                    theLargestSectionIndex <- i
            for i in 0 .. setSizes.Length - 1 do
                if(i <> theLargestSectionIndex) then
                    let x, y = randomSetPoint.[i]

                    let connectPointToSection board x y i =
                        let targetx, targety = searchForClosestMainSectionPoint board x y i 0
                        digTunnel board x y targetx targety i
                    tmpBoard <- connectPointToSection tmpBoard x y (theLargestSectionIndex + 1)
            tmpBoard

// monsters related code

let putRandomMonstersOnBoard (board:Board) =
    let rec findEmptySpotsAndPutMonsters n (board: Board) =
        match n with
        | 0 -> board
        | _ ->
            let x = rnd boardWidth
            let y = rnd boardHeight
            if((isObstacle board (Point (x,y)))) then
                findEmptySpotsAndPutMonsters n board
            else
                findEmptySpotsAndPutMonsters (n-1) (board |> Board.moveCharacter { Type = CharacterType.Monster; Monster =  Some(new Monster(n,'g')) } (new Point(x, y)))
    findEmptySpotsAndPutMonsters 10 board


// test code

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

let addItems board =
    // returns sequence of board modification functions
    let modifiers = seq {
        for i in 1..20 do
            let posX = rnd boardWidth
            let posY = rnd boardHeight
            yield (fun board -> 
                Board.modify (point posX posY) (fun place -> 
                    {place with Items = { Id = i ; Name = "Stick of Doom" ; Class = Stick { Damage = 3 }} :: place.Items} ) board)
    }
    // apply all modification functions on board
    board |>> modifiers

//let addGold board = 
//    let modifiers = seq {
//        for i in 0..20 do
//            let posX = rnd boardWidth
//            let posY = rnd boardHeight
//            let value = rnd2 1 10
//            yield (fun board -> 
//                Board.modify (point posX posY) (fun place -> 
//                    {place with Items = Gold(value) :: place.Items} ) board)
//    }
//    board |>> modifiers

let generateTest: Board = 
    let mutable board = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Floor}
    let rooms = (generateRooms []) |> Seq.take 4 |> Seq.toList
    let rec addRooms rooms board =
        match rooms with
        | [] -> board
        | item::t -> addRooms t <| (item :> IModifier).Modify board 
    addRooms rooms board
    //|> addGold
    //|> addItems

// dungeon generation section

let generateDungeonRooms sections sectionWidth sectionHeight sectionsHorizontal sectionsVertical = 
    let rooms : Tunnel [,] = Array2D.create sectionsHorizontal sectionsVertical (new Tunnel(new Rectangle( 0, 0, 2, 2), true))
    let resultRooms = Array2D.mapi ( fun x y i -> new Tunnel(new Rectangle((x*sectionWidth) + 1 + x, (y*sectionHeight) + 1 + y, sectionWidth, sectionHeight), true)) rooms
    resultRooms

let generateTunnelConnection x1 y1 x2 y2 =
    let horizontalPart = [new Tunnel(new Rectangle(min x1 x2, min y1 y2, abs (x2 - x1) + 1, 1), false)]
    let varticalPart = [new Tunnel(new Rectangle((if (y1 < y2) then x2 else x1), min y1 y2, 1, abs (y2 - y1) + 1), false)]
    List.append horizontalPart varticalPart

let generateDungeonConnections sections (rooms: Tunnel[,]) sectionWidth sectionHeight sectionsHorizontal sectionsVertical =
    let mutable connectionList = []
    for x = 0 to sectionsHorizontal - 1 do 
            for y = 0 to sectionsVertical - 1 do
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

let isGoodPlaceForDoor (board : Board) x y =
    let mutable result = false
    if( x > 0 && x < (boardWidth - 1) && y > 0 && y < (boardHeight - 1) && board.[x,y].Tile = Tile.Floor) then
        if (board.[x,y-1].Tile = Tile.Floor && board.[x,y+1].Tile = Tile.Floor) then
            if (board.[x-1, y].Tile = Tile.Wall && board.[x+1, y].Tile = Tile.Wall) then
                if (board.[x-1,y-1].Tile = Tile.Floor || board.[x+1,y-1].Tile = Tile.Floor || board.[x-1,y+1].Tile = Tile.Floor || board.[x+1,y+1].Tile = Tile.Floor) then
                    result <- true
        if (board.[x-1,y].Tile = Tile.Floor && board.[x+1,y].Tile = Tile.Floor) then
            if (board.[x,y-1].Tile = Tile.Wall && board.[x,y+1].Tile = Tile.Wall) then
                if (board.[x-1,y-1].Tile = Tile.Floor || board.[x+1,y-1].Tile = Tile.Floor || board.[x-1,y+1].Tile = Tile.Floor || board.[x+1,y+1].Tile = Tile.Floor) then
                    result <- true
    result

let addRandomDoors (board : Board) =
    let closedDoor = {Place.EmptyPlace with Tile = Tile.ClosedDoor}
    let floor = {Place.EmptyPlace with Tile = Tile.Floor}
    Array2D.mapi (fun x y i -> if (i = floor && (isGoodPlaceForDoor board x y) && (rnd 100) < 30) then closedDoor else i) board

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
    let resultBoard = addRooms (generateDungeonTunnels sections sectionWidth sectionHeight sectionsHorizontal sectionsVertical) board
    addRandomDoors resultBoard
    //|> addGold
    |> addItems

// dungeon BSP generation method

let createConnectionBetweenClosestRooms (roomsList1 : Tunnel list) (roomsList2 : Tunnel list) =
    let mutable shortestDistance = 1000000
    let mutable bestX1 = 0
    let mutable bestY1 = 0
    let mutable bestX2 = 0
    let mutable bestY2 = 0
    for i1 in 0..(roomsList1.Length - 1) do
        for i2 in 0..(roomsList2.Length - 1) do
            let x1, y1 = roomsList1.[i1].GetRandomPointInside
            let x2, y2 = roomsList2.[i2].GetRandomPointInside
            let calculatedDistance = (abs x1 - x2) + (abs y1 - y2)
            if(calculatedDistance < shortestDistance) then
                shortestDistance <- calculatedDistance
                bestX1 <- x1
                bestY1 <- y1
                bestX2 <- x2
                bestY2 <- y2
    (generateTunnelConnection bestX1 bestY1 bestX2 bestY2)

let rec createTwoConnectedSections x y width height =
    if (width > height) then
        let newx = x + rnd2 (width / 3) ((width / 3) * 2)
        if (width > 20) then
            let section1 = createTwoConnectedSections x y (newx - x) height
            let section2 = createTwoConnectedSections (newx  + 1) y (width - (newx - x) - 1) height
            List.concat [section1 ; (createConnectionBetweenClosestRooms section1 section2) ; section2]
        else
            let room1 = new Tunnel(new Rectangle(x, y, (newx - x), height), true)
            let room1x, room1y = room1.GetRandomPointInside
            let room2 = new Tunnel(new Rectangle(newx + 1, y, (width - (newx - x) - 1), height), false)
            let room2x, room2y = room2.GetRandomPointInside
            List.concat [[room1] ; (generateTunnelConnection room1x room1y room2x room2y) ; [room2] ]
    else
        let newy = y + rnd2 (height / 4) ((height / 4) * 3)
        if(height > 20) then
            let section1 = createTwoConnectedSections x y width (newy - y)
            let section2 = createTwoConnectedSections x (newy + 1) width (height - (newy - y) - 1)
            List.concat [section1 ; (createConnectionBetweenClosestRooms section1 section2) ; section2]
        else
            let room1 = new Tunnel(new Rectangle(x, y, width, (newy - y)), true)
            let room1x, room1y = room1.GetRandomPointInside
            let room2 = new Tunnel(new Rectangle(x, newy + 1, width, (height - (newy - y) - 1)), false)
            let room2x, room2y = room2.GetRandomPointInside
            List.concat [[room1] ; (generateTunnelConnection room1x room1y room2x room2y) ; [room2] ]

let generateBSPDungeon =
    let rec addRooms rooms board =
        match rooms with
        | [] -> board
        | item::t -> addRooms t <| (item :> IModifier).Modify board 

    let mutable board = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Wall}
    let tunnelsList = createTwoConnectedSections 1 1 (boardWidth - 2) (boardHeight - 2)
    addRooms tunnelsList board

//cave generation section
    
let generateCave: Board = 
    let mutable board = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Floor}
    board <- scatterTilesRamdomlyOnBoard board Tile.Wall Tile.Floor 0.5 true
    board <- smoothOutTheLevel board 2 Tile.Wall Tile.Floor (4,5)
    let sections = new DisjointLocationSet(board, Tile.Floor)
    sections.ConnectUnconnected
    //|> addGold
    |> addItems
    |> putRandomMonstersOnBoard

// jungle/forest generation

let generateForest: Board =
    let mutable board = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Grass}
    board <- scatterTilesRamdomlyOnBoard board Tile.Tree Tile.Grass 0.25 true
    board <- smoothOutTheLevel board 1 Tile.Tree Tile.Grass (1,4)
    let sections = new DisjointLocationSet(board, Tile.Grass)
    board <- sections.ConnectUnconnected
    board <- scatterTilesRamdomlyOnBoard board Tile.Bush Tile.Grass 0.05 false
    board <- scatterTilesRamdomlyOnBoard board Tile.SmallPlants Tile.Grass 0.05 false
    board

// main level generation switch
let generateLevel levelType : Board = 
    match levelType with
    | LevelType.Test -> generateTest
    | LevelType.Dungeon -> generateDungeon// generateBSPDungeon //generateDungeon
    | LevelType.Cave -> generateCave
    | LevelType.Forest -> generateForest
    | _ -> failwith "unknown level type"

