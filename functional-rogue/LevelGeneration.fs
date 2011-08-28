module LevelGeneration

open System.Drawing
open Board
open Items
open Monsters
open Characters
open Config
open Resources

// predefined parts

let simplifiedObjectToMapPart (input: char[,]) (background: Tile) =
    let backgroundTile = { Tile = background; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = Option.None }
    let wall = { Tile = Tile.Wall; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = Option.None }
    let glass = { Tile = Tile.Glass; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = Option.None }
    let closedDoor = { Tile = Tile.ClosedDoor; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = Option.None }
    let floor = { Tile = Tile.Floor; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = Option.None }
    let stairsDown = { Tile = Tile.StairsDown; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = Option.None }
    let stairsUp = { Tile = Tile.StairsUp; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = Option.None }
    input |> Array2D.map (fun i ->
        match i with
        | '0' -> backgroundTile
        | '#' -> wall
        | 'g' -> glass
        | '+' -> closedDoor
        | '.' -> floor
        | '>' -> stairsDown
        | '<' -> stairsUp
        | _ -> backgroundTile
    )

let generateStartingLevelShip (background: Tile) =
    let simplifiedShip = ResourceManager.Instance.SimplifiedMapObjects.["StartLocationShip"]
    simplifiedObjectToMapPart simplifiedShip background

// level generation utilities

let scatterTilesRamdomlyOnBoard (board: Board) (tileToPut:Tile) (backgroundTile:Tile) (probability:float) (createBorder:bool) : Board =
    let background = {Place.EmptyPlace with Tile = backgroundTile}
    let placeToPut = {Place.EmptyPlace with Tile = tileToPut}
    let newPlaces = board.Places |> Array2D.mapi (fun x y i ->
        if(not(i.Tile = backgroundTile)) then
            i
        elif(createBorder && (x = 0 || x = (boardWidth - 1) || y = 0 || y = (boardHeight - 1))) then
            placeToPut
        else
            if((float)(rnd 100) < (probability * (float)100)) then placeToPut else i
           ) 
    { board with Places = newPlaces }

let placeStairsUp (backgroundTile:Tile) (cameFrom: TransportTarget) (board: Board) =
    let rec getRandomBackgroundPlace () =
        let x = rnd2 1 (boardWidth - 2)
        let y = rnd2 1 (boardHeight - 2)
        if (board.Places.[x,y].Tile = backgroundTile) then
            Point(x,y)
        else
            getRandomBackgroundPlace ()
    let stairsPoint = getRandomBackgroundPlace()
    let result = Board.set (stairsPoint) {Place.EmptyPlace with Tile = Tile.StairsUp; TransportTarget = Some(cameFrom)} board
    (result, stairsPoint)

let maybePlaceStairsDown (backgroundTile:Tile) (level: int) (board: Board) =
    if (rnd 100) > (level*(-2)*10) then
        let rec getRandomBackgroundPlace () =
            let x = rnd2 1 (boardWidth - 2)
            let y = rnd2 1 (boardHeight - 2)
            if (board.Places.[x,y].Tile = backgroundTile) then
                Point(x,y)
            else
                getRandomBackgroundPlace ()
        let stairsPoint = getRandomBackgroundPlace()
        let result = Board.set (stairsPoint) {Place.EmptyPlace with Tile = Tile.StairsDown; TransportTarget = Option.None} board
        result
    else
        board

let countTileNeighbours (places: Place[,]) x y (tileType:Tile)=
    let tileToSearch = {Place.EmptyPlace with Tile = tileType}
    let mutable count = 0
    for tmpx in x - 1 .. x + 1 do
        for tmpy in y - 1 .. y + 1 do
            if not(tmpx = x && tmpy = y) then
                count <- count + (if(places.[tmpx,tmpy] = tileToSearch) then 1 else 0)
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
     let mutable flags = Array2D.create (Array2D.length1 board.Places) (Array2D.length2 board.Places) -1
     let mutable current = 0

     let createSets =
        let rec floodLocation (board: Board) (flags: int[,]) x y current =
            let mutable tmpFlags = flags
            if(board.Places.[x,y].Tile = basicTile && tmpFlags.[x,y] = -1) then
                Array2D.set tmpFlags x y current
                for floodx in (max (x - 1) 0) .. (min (x + 1) ((Array2D.length1 board.Places) - 1)) do
                    for floody in (max (y - 1) 0) .. (min (y + 1) ((Array2D.length2 board.Places) - 1)) do
                        tmpFlags <- floodLocation board tmpFlags floodx floody current
            tmpFlags

        for x in 0 .. (Array2D.length1 board.Places) - 1 do
            for y in 0 .. (Array2D.length2 board.Places) - 1 do
                if(board.Places.[x,y].Tile = basicTile && flags.[x,y] = -1) then
                    current <- current + 1
                    flags <- (floodLocation board flags x y current)

     let mutable setSizes: int[] = Array.create current 0
     let mutable randomSetPoint = Array.create current (0,0)
     
     let countSizes =
        for x in 0 .. (Array2D.length1 board.Places) - 1 do
            for y in 0 .. (Array2D.length2 board.Places) - 1 do
                if(flags.[x,y] > 0) then
                    setSizes.[flags.[x,y] - 1] <- (setSizes.[flags.[x,y] - 1] + 1)
                    randomSetPoint.[flags.[x,y] - 1] <- (x,y)
     
     member this.NumberOfSections
        with get() = current

     member this.ConnectUnconnected : Board =
            let searchForMainSectionPiont (board: Board) x y i depth =
                let mutable result = (-1,-1)
                let minX = max (x - depth) 0
                let maxX = (min (x + depth) ((Array2D.length1 board.Places) - 1))
                let minY = max (y - depth) 0
                let maxY = (min (y + depth) ((Array2D.length2 board.Places) - 1))
                for searchx in minX .. maxX do
                    for searchy in minY .. maxY do
                        if (searchx = minX || searchx = maxX) || (searchy = minY || searchy = maxY) then
                            if (flags.[searchx,searchy] = i) then
                                result <- (searchx,searchy)
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
                        digTunnel board.Places x y targetx targety i
                    tmpBoard <- { tmpBoard with Places = connectPointToSection tmpBoard x y (theLargestSectionIndex + 1) }
            tmpBoard

// monsters related code

let getRandomMonsterType () =
        let cases = Reflection.FSharpType.GetUnionCases(typeof<MonsterType>)
        let index = rnd cases.Length
        let case = cases.[index]
        Reflection.FSharpValue.MakeUnion(case, [||]) :?> MonsterType

let putRandomMonstersOnBoard (board:Board) =
    let rec findEmptySpotsAndPutMonsters n (board: Board) =
        match n with
        | 0 -> board
        | _ ->
            let x = rnd boardWidth
            let y = rnd boardHeight
            if((isMovementObstacle board (Point (x,y)))) then
                findEmptySpotsAndPutMonsters n board
            else
                findEmptySpotsAndPutMonsters (n-1) (board |> Board.moveCharacter (createNewMonster(getRandomMonsterType ())) (new Point(x, y)))
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
    let stickOfDoom = {
        Id = 0;
        Name = "Stick of doom";
        Wearing = {
                    OnHead = false;
                    InHand = true;
                    OnTorso = false;
                    OnLegs = true
        };
        Offence = Value(3M);
        Defence = Value(0M);
        Type = Stick
    }
    let modifiers = seq {
        for i in 1..20 do
            let posX = rnd boardWidth
            let posY = rnd boardHeight
            yield (fun board -> 
                Board.modify (point posX posY) (fun place -> 
                    {place with Items = { stickOfDoom with Id = i } :: place.Items} ) board)
    }
    // apply all modification functions on board
    board |>> modifiers

let addOre board = 
    let modifiers = seq {
        for i in 0..20 do
            let posX = rnd boardWidth
            let posY = rnd boardHeight
            let value = rnd2 1 10
            yield (fun board -> 
                Board.modify (point posX posY) (fun place -> 
                    {place with Ore = Uranium(value)} ) board)
    }
    board |>> modifiers

let generateTest: (Board*Point option) = 
    let mutable board = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Floor}
    let rooms = (generateRooms []) |> Seq.take 4 |> Seq.toList
    let rec addRooms rooms board =
        match rooms with
        | [] -> board
        | item::t -> addRooms t <| (item :> IModifier).Modify board 
    (((addRooms rooms { Guid = System.Guid.NewGuid(); Places = board; Level = 0; MainMapLocation = Option.None}) |> addOre |> addItems |> putRandomMonstersOnBoard),Option.None)

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
    if( x > 0 && x < (boardWidth - 1) && y > 0 && y < (boardHeight - 1) && board.Places.[x,y].Tile = Tile.Floor) then
        if (board.Places.[x,y-1].Tile = Tile.Floor && board.Places.[x,y+1].Tile = Tile.Floor) then
            if (board.Places.[x-1, y].Tile = Tile.Wall && board.Places.[x+1, y].Tile = Tile.Wall) then
                if (board.Places.[x-1,y-1].Tile = Tile.Floor || board.Places.[x+1,y-1].Tile = Tile.Floor || board.Places.[x-1,y+1].Tile = Tile.Floor || board.Places.[x+1,y+1].Tile = Tile.Floor) then
                    result <- true
        if (board.Places.[x-1,y].Tile = Tile.Floor && board.Places.[x+1,y].Tile = Tile.Floor) then
            if (board.Places.[x,y-1].Tile = Tile.Wall && board.Places.[x,y+1].Tile = Tile.Wall) then
                if (board.Places.[x-1,y-1].Tile = Tile.Floor || board.Places.[x+1,y-1].Tile = Tile.Floor || board.Places.[x-1,y+1].Tile = Tile.Floor || board.Places.[x+1,y+1].Tile = Tile.Floor) then
                    result <- true
    result

let addRandomDoors (board : Board) =
    let closedDoor = {Place.EmptyPlace with Tile = Tile.ClosedDoor}
    let floor = {Place.EmptyPlace with Tile = Tile.Floor}
    { board with Places = Array2D.mapi (fun x y i -> if (i = floor && (isGoodPlaceForDoor board x y) && (rnd 100) < 30) then closedDoor else i) board.Places }

let generateDungeon: (Board*Point option) = 
    let mutable board = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Wall}
    let sectionsHorizontal = 4
    let sectionsVertical = 3
    let sectionWidth = ((boardWidth - 1) / sectionsHorizontal) - 1
    let sectionHeight = ((boardHeight - 1) / sectionsVertical) - 1
    let sections = [for x in 0..(sectionsHorizontal - 1) do for y in 0..(sectionsVertical - 1) do yield (x, y)]

    let rec addRooms rooms (board: Board) =
        match rooms with
        | [] -> board
        | item::t -> addRooms t <| (item :> IModifier).Modify board 
    let resultBoard = addRooms (generateDungeonTunnels sections sectionWidth sectionHeight sectionsHorizontal sectionsVertical) { Guid = System.Guid.NewGuid(); Places = board; Level = 0; MainMapLocation = Option.None}
    (addRandomDoors resultBoard
    |> addOre
    |> addItems, Option.None)

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

    let mutable board = { Guid = System.Guid.NewGuid(); Places = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Wall}; Level = 0; MainMapLocation = Option.None}
    let tunnelsList = createTwoConnectedSections 1 1 (boardWidth - 2) (boardHeight - 2)
    addRooms tunnelsList board

//cave generation section
    
let generateCave (cameFrom: TransportTarget option) (level: int) : (Board*Point option) =
    let mutable board =  { Guid = System.Guid.NewGuid(); Places = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Floor}; Level = level; MainMapLocation = Option.None}
    board <- scatterTilesRamdomlyOnBoard board Tile.Wall Tile.Floor 0.5 true
    board <- { board with Places = smoothOutTheLevel board.Places 2 Tile.Wall Tile.Floor (4,5) }
    let sections = new DisjointLocationSet(board, Tile.Floor)
    let initial = sections.ConnectUnconnected |> addItems |> addOre
    if (cameFrom.IsSome) then
        let resultBoard, startpoint = initial |> placeStairsUp Tile.Floor cameFrom.Value
        (resultBoard
        |> putRandomMonstersOnBoard
        |> maybePlaceStairsDown Tile.Floor level, Some(startpoint))
    else
        (initial |> putRandomMonstersOnBoard, Option.None)

// jungle/forest generation

let generateForest (cameFrom:Point) : (Board*Point option) =
    let mutable board = { Guid = System.Guid.NewGuid(); Places = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Grass}; Level = 0; MainMapLocation = Some(cameFrom)}
    board <- scatterTilesRamdomlyOnBoard board Tile.Tree Tile.Grass 0.25 false
    board <- { board with Places = smoothOutTheLevel board.Places 1 Tile.Tree Tile.Grass (1,4) }
    let sections = new DisjointLocationSet(board, Tile.Grass)
    board <- sections.ConnectUnconnected
    board <- scatterTilesRamdomlyOnBoard board Tile.Bush Tile.Grass 0.05 false
    board <- scatterTilesRamdomlyOnBoard board Tile.SmallPlants Tile.Grass 0.05 false
    (board, Some(Point(35,15)))

let generateStartLocationWithInitialPlayerPositon (cameFrom:Point) : (Board*Point) =
    let result, startpoint = generateForest cameFrom
    let ship = generateStartingLevelShip Tile.Grass
    Array2D.blit ship 0 0 result.Places 30 10 (Array2D.length1 ship) (Array2D.length2 ship)
    (result,(Point(33,12)))

let generateMainMap: (Board*Point) =
    let mutable board = { Guid = System.Guid.NewGuid(); Places = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.MainMapGrassland}; Level = 0; MainMapLocation = Option.None}
    board <- scatterTilesRamdomlyOnBoard board Tile.MainMapForest Tile.MainMapGrassland 0.25 true
    (board,Point(4,4))

let generateEmpty : Board =
    Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Floor}
    |> Board.modify (new Point(8, 8)) (fun _ -> {Place.EmptyPlace with Tile = Tile.Wall})
    |> Board.modify (new Point(8, 9)) (fun _ -> {Place.EmptyPlace with Tile = Tile.Wall})
    //|> Board.modify (new Point(8, 9)) (fun _ -> {Place.EmptyPlace with Tile = Tile.Wall})

// main level generation switch
let generateLevel levelType (cameFrom: TransportTarget option) (level: int option) : (Board*Point option) = 
    match levelType with
    | LevelType.Test -> generateTest
    | LevelType.Dungeon -> generateDungeon// generateBSPDungeon //generateDungeon
    | LevelType.Cave -> generateCave cameFrom (defaultArg level 0)
    | LevelType.Forest -> generateForest cameFrom.Value.TargetCoordinates
    | LevelType.Empty -> generateEmpty

