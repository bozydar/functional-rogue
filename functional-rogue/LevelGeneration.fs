module LevelGeneration

open System.Drawing
open Board

open Monsters
open Characters
open Config
open Quantity
open State
open Predefined
open Predefined.Items

// predefined parts

let simplifiedObjectToMapPart (input: char[,]) (background: Tile) =
    let backgroundTile = { Tile = background; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = None; ElectronicMachine = None }
    let wall = { Tile = Tile.Wall; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = None; ElectronicMachine = None }
    let glass = { Tile = Tile.Glass; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = None; ElectronicMachine = None }
    let closedDoor = { Tile = Tile.ClosedDoor; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = None; ElectronicMachine = None }
    let floor = { Tile = Tile.Floor; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = None; ElectronicMachine = None }
    let stairsDown = { Tile = Tile.StairsDown; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = None; ElectronicMachine = None }
    let stairsUp = { Tile = Tile.StairsUp; Items = []; Ore = Ore.NoneOre; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; TransportTarget = None; ElectronicMachine = None }
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

// level generation utilities

let noise (x: float) (y: float) =
    let n= (float)((x + y * 57.0) * (2.0*13.0))
    ( 1.0 - ( ( n * (n * n * 15731.0 + 789221.0) + 1376312589.0) % 2147483641.0) / 1073741824.0 )

let applyMaskModifier (source: float[,]) =
    let halfWidth = (float)boardWidth/2.0
    let halfHeight = (float)boardHeight/2.0
    source |> Array2D.mapi (fun x y i ->
        let resultx = if ((float)x < halfWidth ) then (float)x/halfWidth else 2.0 - (float)x/halfWidth
        let resulty = if ((float)y < halfHeight ) then (float)y/halfHeight else 2.0 - (float)y/halfHeight
        if(x = 0 || x = (boardWidth-1) || y = 0 || y = (boardHeight-1)) then
            0.0
        else
            ( (i+1.0) * resultx * resulty )
        ) 

let smoothOutNoise (noiseArray: float[,]) : float[,] = 
    let getCornersValues x y =
        (noiseArray.[x-1,y-1] + noiseArray.[x-1,y+1] + noiseArray.[x+1,y-1] + noiseArray.[x+1,y+1]) / 16.0
    let getSidesValues x y =
        (noiseArray.[x,y-1] + noiseArray.[x,y+1] + noiseArray.[x-1,y] + noiseArray.[x+1,y]) / 8.0
    Array2D.mapi (fun x y i -> if(x = 0 || x = (boardWidth-1) || y = 0 || y = (boardHeight-1)) then i else (getCornersValues x y) + (getSidesValues x y) + (i/4.0)) noiseArray

let perlinNoise (x: int) (y: int) =
    let startFrequency = 32.0
    let startAmplitude = 1.0/1024.0
    let rec total (f: float) (a: float) =
        match f with
        | 1.0 -> (noise ((float)x*f) ((float)y*f)) * a
        | _ ->
            let freq = (f/2.0)
            let amplitude = a*4.0
            ((noise ((float)x*f) ((float)y*f)) * a) + total freq amplitude
    total startFrequency startAmplitude

let rec getRandomBackgroundPlace (backgroundTile: Tile) (board: Board) =
            let x = rnd2 1 (boardWidth - 2)
            let y = rnd2 1 (boardHeight - 2)
            if (board.Places.[x,y].Tile = backgroundTile) then
                Point(x,y)
            else
                getRandomBackgroundPlace backgroundTile board

let scatterTilesRamdomlyOnBoard (tileToPut:Tile) (backgroundTile:Tile) (probability:float) (createBorder:bool) (board: Board) : Board =
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
    let stairsPoint = getRandomBackgroundPlace backgroundTile board
    let result = Board.set (stairsPoint) {Place.EmptyPlace with Tile = Tile.StairsUp; TransportTarget = Some(cameFrom)} board
    (result, stairsPoint)

let maybePlaceStairsDown (backgroundTile:Tile) (level: int) (board: Board) =
    if (rnd 100) > (level*(-2)*10) then
        let stairsPoint = getRandomBackgroundPlace backgroundTile board
        let result = Board.set (stairsPoint) {Place.EmptyPlace with Tile = Tile.StairsDown; TransportTarget = Option.None} board
        result
    else
        board

let maybePlaceCaveEntrance (backgroundTile:Tile) (probability:float) (board: Board) =
    if((float)(rnd 100) < (probability * (float)100)) then
        let entrancePoint = getRandomBackgroundPlace backgroundTile board
        let result = Board.set (entrancePoint) {Place.EmptyPlace with Tile = Tile.StairsDown; TransportTarget = Option.None} board
        result
    else
        board

let maybePlaceNonNaturalObjects (probability:float) (board: Board) =
    if((float)(rnd 100) < (probability * (float)100) && State.stateExists()) then   //stateExists prevents from creating a non natural object on the starting map (the crash site) as it is created before the state is initialized
        board |> Predefined.Resources.randomAncientRuins
    else
        board

let maybePlaceSomeOre (backgroundTile:Tile) (level: int) (board: Board) =
    if (rnd 100) < (min 90 (level*(-2)*10)) then // probablity for underground levels 20%, 40%, 60%, 80%, 90%, 90%, ...
        let getkaka = Ore.Iron
        let ss = getkaka((QuantityValue)4)
        let getRandomOreKind =
            let randomResult = rnd 100
            if (randomResult < 20) then Ore.Uranium // 20%
            else if (randomResult < 50) then Ore.Gold   //30%
            else Ore.Iron   //50%

        let rec placeRandomOres (amount: int) (board: Board) =
            match amount with
            | 0 -> board
            | _ ->
                let orePoint = getRandomBackgroundPlace backgroundTile board
                let orePlace = Board.get board orePoint
                placeRandomOres (amount - 1) (Board.set orePoint { orePlace with Ore = getRandomOreKind(Quantity.QuantityValue (rnd 8))} board)
        placeRandomOres (rnd 10) board
    else
        board

let placeSomeRandomItems (backgroundTile:Tile) (levelType: LevelType) (board: Board) =
    let rec placeRandomItems (amount: int) (board: Board) =
        match amount with
        | 0 -> board
        | _ ->
            let itemPoint = getRandomBackgroundPlace backgroundTile board
            let itemPlace = Board.get board itemPoint
            let item =
                match levelType with
                | Cave | Forest | Grassland | Coast ->
                    [Items.createRandomNaturalItem 0]
                | _ -> []
            placeRandomItems (amount - 1) (Board.set itemPoint { itemPlace with Items = itemPlace.Items @ item} board)
    placeRandomItems (rnd2 4 10) board

let placeLake (backgroundTile:Tile) (board: Board) =
    let rec getRandomBackgroundPlaceNotTooCloseToBorder () =
        let x = rnd2 6 (boardWidth - 7)
        let y = rnd2 6 (boardHeight - 7)
        if (board.Places.[x,y].Tile = backgroundTile) then
            Point(x,y)
        else
            getRandomBackgroundPlaceNotTooCloseToBorder ()
    let rec growRandomLake (currentPoint: Point) (size: int) (waterType: Ore) (board: Board) =
        match size with
        | 0 -> board
        | _ ->
            let thePlace = Board.get board currentPoint
            let nextPoint = Point(min (boardWidth - 1) (max 0 (currentPoint.X + (rnd 3) - 1)), min (boardHeight - 1) (max 0 (currentPoint.Y + (rnd 3) - 1)))
            growRandomLake nextPoint (size - 1) waterType (Board.set currentPoint { thePlace with Tile = Tile.Water; Ore = waterType } board)
    let randomWaterType =
        let number = rnd 100
        if number < 20 then
            Ore.CleanWater Quantity.PositiveInfinity    //20% chance for clean water in a lake
        else
            Ore.ContaminatedWater Quantity.PositiveInfinity //80% chance for contaminated water in a lake
    let startPoint = getRandomBackgroundPlaceNotTooCloseToBorder()
    growRandomLake startPoint 25 randomWaterType board

let placeStream (backgroundTile:Tile) (board: Board) =
    let horizontalVariation = rnd 3
    let verticalVariation = if (horizontalVariation = 1) then (if (rnd 2) = 0 then 0 else 2) else rnd 3
    let startPointX =
        match horizontalVariation with
        | 1 -> (rnd (boardWidth - 5)) + 4
        | _ -> if(horizontalVariation = 0) then 0 else (boardWidth - 1)
    let startPointY =
        match verticalVariation with
        | 1 -> (rnd (boardHeight - 5)) + 4
        | _ -> if(verticalVariation = 0) then 0 else (boardHeight - 1)
    let randomWaterType =
        let number = rnd 100
        if number < 70 then
            Ore.CleanWater Quantity.PositiveInfinity    //70% chance for clean water in a stream
        else
            Ore.ContaminatedWater Quantity.PositiveInfinity //30% chance for contaminated water in a stream
    let rec growRandomStream (currentPoint: Point) (waterType: Ore) (board: Board) =
        if (currentPoint.X = -1 || currentPoint.X = boardWidth || currentPoint.Y = -1 || currentPoint.Y = boardHeight) then
            board
        else
            let thePlace = Board.get board currentPoint
            let nextX = currentPoint.X + (min 1 (max (-1) ((rnd 3) - horizontalVariation)))
            let nexyY = currentPoint.Y + (min 1 (max (-1) ((rnd 3) - verticalVariation)))
            let nextPoint = Point(nextX , nexyY)
            growRandomStream nextPoint waterType (Board.set currentPoint { thePlace with Tile = Tile.Water; Ore = waterType } board)
    growRandomStream (Point(startPointX,startPointY)) randomWaterType board

let maybePlaceSomeWater (backgroundTile:Tile) (probability:float) (board: Board) =
    if((float)(rnd 100) < (probability * (float)100)) then
        if (rnd 2) = 0 then
            placeLake backgroundTile board  // 50% chance that it's a lake
        else
            placeStream backgroundTile board // 50% chance that it's a stream
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

let rec smoothOutTheLevel howManyTimes (tileToGrow:Tile) (backgroundTile:Tile) (rule:(int*int)) board =
    let background = {Place.EmptyPlace with Tile = backgroundTile}
    let toGrow = {Place.EmptyPlace with Tile = tileToGrow}
    let min, max = rule
    let rec smoothOutPlaces places howManyTimes toGrow background min max =
        match howManyTimes with
        | 0 -> places
        | _ -> smoothOutPlaces 
                (Array2D.mapi (fun x y i -> 
                if(x > 0 && x < (boardWidth - 1) && y > 0 && y < (boardHeight - 1)) then
                    if((countTileNeighbours places x y tileToGrow) < min ) then background
                    elif ((countTileNeighbours places x y tileToGrow) > max) then toGrow
                    else i
                else i
                    ) places)
                (howManyTimes - 1) toGrow background min max
    { board with Places = smoothOutPlaces board.Places howManyTimes toGrow background min max }


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
    //let stickOfDoom = {
    //    Id = System.Guid.NewGuid();
    //    Name = "Stick of doom";
    //    Wearing = {
    //                OnHead = false;
    //                InHand = true;
    //                OnTorso = false;
    //                OnLegs = true
    //    };
    //    Offence = Value(3M);
    //    Defence = Value(0M);
    //    Type = Stick;
    //    MiscProperties = Items.defaultMiscProperties
    //}
    let modifiers = seq {
        for i in 1..20 do
            let posX = rnd boardWidth
            let posY = rnd boardHeight
            yield (fun board -> 
                Board.modify (point posX posY) (fun place -> 
                    {place with Items = Items.stickOfDoom :: place.Items} ) board)
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
                    {place with Ore = Uranium(QuantityValue(value))} ) board)
    }
    board |>> modifiers

let generateTest: (Board*Point option) = 
    let mutable board = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Floor}
    let rooms = (generateRooms []) |> Seq.take 4 |> Seq.toList
    let rec addRooms rooms board =
        match rooms with
        | [] -> board
        | item::t -> addRooms t <| (item :> IModifier).Modify board 
    (((addRooms rooms { Guid = System.Guid.NewGuid(); Places = board; Level = 0; MainMapLocation = Option.None; Type = LevelType.Test}) |> addOre |> addItems |> putRandomMonstersOnBoard),Option.None)

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

let generateDungeon (cameFrom: TransportTarget option) (level: int) : (Board*Point option) = 
    let board = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Wall}
    let sectionsHorizontal = 4
    let sectionsVertical = 3
    let sectionWidth = ((boardWidth - 1) / sectionsHorizontal) - 1
    let sectionHeight = ((boardHeight - 1) / sectionsVertical) - 1
    let sections = [for x in 0..(sectionsHorizontal - 1) do for y in 0..(sectionsVertical - 1) do yield (x, y)]

    let rec addRooms rooms (board: Board) =
        match rooms with
        | [] -> board
        | item::t -> addRooms t <| (item :> IModifier).Modify board 
    let resultBoard = addRooms (generateDungeonTunnels sections sectionWidth sectionHeight sectionsHorizontal sectionsVertical) { Guid = System.Guid.NewGuid(); Places = board; Level = level; MainMapLocation = Option.None; Type = LevelType.Dungeon}
    let finalBoard, startpoint = resultBoard |> addRandomDoors |> placeStairsUp Tile.Floor cameFrom.Value
    (finalBoard
        |> placeSomeRandomItems Tile.Floor LevelType.Dungeon
     , Some(startpoint))

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

    let mutable board = { Guid = System.Guid.NewGuid(); Places = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Wall}; Level = 0; MainMapLocation = Option.None; Type = LevelType.Dungeon}
    let tunnelsList = createTwoConnectedSections 1 1 (boardWidth - 2) (boardHeight - 2)
    addRooms tunnelsList board

//cave generation section
    
let generateCave (cameFrom: TransportTarget option) (level: int) : (Board*Point option) =
    let board =
        { Guid = System.Guid.NewGuid(); Places = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Floor}; Level = level; MainMapLocation = Option.None; Type = LevelType.Cave}
        |> scatterTilesRamdomlyOnBoard Tile.Wall Tile.Floor 0.5 true
        |> smoothOutTheLevel 2 Tile.Wall Tile.Floor (4,5)
    let sections = new DisjointLocationSet(board, Tile.Floor)
    let initial =
        sections.ConnectUnconnected
        |> placeSomeRandomItems Tile.Floor LevelType.Cave
        |> maybePlaceSomeOre Tile.Floor level
    if (cameFrom.IsSome) then
        let resultBoard, startpoint = initial |> placeStairsUp Tile.Floor cameFrom.Value
        (resultBoard
        |> putRandomMonstersOnBoard
        |> maybePlaceStairsDown Tile.Floor level
        , Some(startpoint))
    else
        (initial |> putRandomMonstersOnBoard |> maybePlaceSomeOre Tile.Floor level, Option.None)

// jungle/forest generation

let generateForest (cameFrom:Point) : (Board*Point option) =
    let board =
        { Guid = System.Guid.NewGuid(); Places = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Grass}; Level = 0; MainMapLocation = Some(cameFrom); Type = LevelType.Forest}
        |> maybePlaceSomeWater Tile.Grass 0.15
        |> maybePlaceCaveEntrance Tile.Grass 0.10
        |> scatterTilesRamdomlyOnBoard Tile.Tree Tile.Grass 0.25 false
        |> smoothOutTheLevel 1 Tile.Tree Tile.Grass (1,4)
    let sections = new DisjointLocationSet(board, Tile.Grass)
    let resultBoard =
        sections.ConnectUnconnected
        |> placeSomeRandomItems Tile.Grass LevelType.Forest
        |> scatterTilesRamdomlyOnBoard Tile.Bush Tile.Grass 0.05 false
        |> scatterTilesRamdomlyOnBoard Tile.SmallPlants Tile.Grass 0.05 false
        |> maybePlaceNonNaturalObjects 0.1
    (resultBoard, Some(Point(35,15)))

let generateGrassland (cameFrom:Point) : (Board*Point option) =
    let board =
        { Guid = System.Guid.NewGuid(); Places = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Grass}; Level = 0; MainMapLocation = Some(cameFrom); Type = LevelType.Grassland}
        |> maybePlaceSomeWater Tile.Grass 0.25
        |> maybePlaceCaveEntrance Tile.Grass 0.05
        |> scatterTilesRamdomlyOnBoard Tile.Tree Tile.Grass 0.01 false
        |> placeSomeRandomItems Tile.Grass LevelType.Grassland
        |> scatterTilesRamdomlyOnBoard Tile.Bush Tile.Grass 0.05 false
        |> scatterTilesRamdomlyOnBoard Tile.SmallPlants Tile.Grass 0.05 false
    (board, Some(Point(35,15)))

let generateCoast (cameFrom:Point) : (Board*Point option) =
    let board =
        { Guid = System.Guid.NewGuid(); Places = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Sand}; Level = 0; MainMapLocation = Some(cameFrom); Type = LevelType.Coast}
        |> maybePlaceSomeWater Tile.Sand 0.35
        |> scatterTilesRamdomlyOnBoard Tile.Tree Tile.Sand 0.01 false
        |> placeSomeRandomItems Tile.Sand LevelType.Coast
        |> scatterTilesRamdomlyOnBoard Tile.Bush Tile.Sand 0.05 false
        |> scatterTilesRamdomlyOnBoard Tile.SmallPlants Tile.Sand 0.05 false
    (board, Some(Point(35,15)))

let generateStartLocationWithInitialPlayerPositon (cameFrom:Point) : (Board*Point) =
    let board, startpoint = generateForest cameFrom
    let result =
        board
        |> Predefined.Resources.startLocationShip
    //TODO: Delete the line below - it's for testing only
    result.Places.[0,0] <- { result.Places.[0,0] with ElectronicMachine = Some( { ComputerContent = { ComputerName = "Upper left camera"; Notes = []; CanOperateDoors = false; CanOperateCameras = false; CanReplicate = false; HasCamera = true; ReplicationRecipes = [] } } )}
    (result,(Point(32,12)))

let generateMainMap: (Board*Point) =
    let noiseValueToMap (value: float) =
        if value < 0.12 then Tile.MainMapWater
        else if value < 0.17 then Tile.MainMapCoast
        else if value > 0.9 then Tile.MainMapMountains
        else if value > 0.3 then Tile.MainMapForest
        else Tile.MainMapGrassland
     
    let noise = Array2D.init boardWidth boardHeight (fun x y -> perlinNoise x y)   
    let smoothNoise = smoothOutNoise (smoothOutNoise noise)
    let withMask = applyMaskModifier smoothNoise

    let board = { Guid = mainMapGuid; Places = Array2D.init boardWidth boardHeight (fun x y -> {Place.EmptyPlace with Tile = noiseValueToMap (withMask.[x,y])}); Level = 0; MainMapLocation = Option.None; Type = LevelType.MainMap}

    //TODO: this to be deleted later... for this is map file generation for dev purposes
    let tileToStr (tile:Tile) =
        match tile with
        | Tile.MainMapCoast -> "."
        | Tile.MainMapForest -> "&"
        | Tile.MainMapGrassland -> "\""
        | Tile.MainMapMountains -> "^"
        | Tile.MainMapWater -> "~"
        | _ -> " "

    let strings =
        let mutable result : string list = []
        for y in 0..(boardHeight-1) do
            let mutable line = ""
            for x in 0..(boardWidth-1) do
                line <- line + tileToStr(board.Places.[x,y].Tile)
            result <- result @ [line]
        result
    System.IO.File.WriteAllLines("C:\\tratata.txt", strings)
    //TODO: later delete the above

    let rec findRandomStartLocation board =
        let x = rnd boardWidth
        let y = rnd boardHeight
        if (board.Places.[x,y].Tile = Tile.MainMapForest) then
            Point(x,y)
        else findRandomStartLocation board

    (board,(findRandomStartLocation board))

let generateEmpty : (Board * Point option) =
    let places = 
        Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Floor}        
    let board = 
        { Guid = System.Guid.NewGuid(); Places = places; Level = 0; MainMapLocation = Option.None; Type = LevelType.Test}
        |> Board.modify (new Point(8, 8)) (fun _ -> {Place.EmptyPlace with Tile = Tile.Wall})
        |> Board.modify (new Point(8, 9)) (fun _ -> {Place.EmptyPlace with Tile = Tile.Wall})
    (board, Option.Some(new Point(5,5)))
    //|> Board.modify (new Point(8, 9)) (fun _ -> {Place.EmptyPlace with Tile = Tile.Wall})

let generateTestStartLocationWithInitialPlayerPositon (cameFrom:Point) : (Board*Point) =
    let board, startpoint = generateEmpty
    let result = board |> Predefined.Resources.randomAncientRuins
    result.Places.[33,13] <- { result.Places.[33,13] with Items = List.replicate 15 (Predefined.Items.createRandomNaturalItem 0) }
    result.Places.[33,14] <- { result.Places.[33,14] with Items = [createReconnaissanceDrone()] }
    let injectorWithSolution = createEmptyMedicalInjector() |> fillContainerWithLiquid LiquidType.HealingSolution
    result.Places.[33,15] <- { result.Places.[33,15] with Items = [injectorWithSolution] }
    let emptyInjector = createEmptyMedicalInjector()
    result.Places.[33,16] <- { result.Places.[33,16] with Items = [createEmptyCanteen() |> fillContainerWithLiquid LiquidType.Water; emptyInjector] }
    (result,(Point(32,12)))


// main level generation switch
let generateLevel levelType (cameFrom: TransportTarget option) (level: int option) : (Board*Point option) = 
    match levelType with
    | LevelType.Test -> generateTest
    | LevelType.Dungeon -> generateDungeon cameFrom (defaultArg level 0) // generateBSPDungeon //generateDungeon
    | LevelType.Cave -> generateCave cameFrom (defaultArg level 0)
    | LevelType.Forest -> generateForest cameFrom.Value.TargetCoordinates
    | LevelType.Grassland -> generateGrassland cameFrom.Value.TargetCoordinates
    | LevelType.Coast -> generateCoast cameFrom.Value.TargetCoordinates
    | LevelType.Empty -> generateEmpty
    | LevelType.MainMap ->
        let map, point = generateMainMap
        (map, Some(point))

