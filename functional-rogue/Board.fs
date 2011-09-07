module Board

open System
open System.Drawing
open Utils
open Config
open Items
open Monsters
open Characters
open Quantity

type Tile =
    | Wall 
    | Floor
    | Empty
    | OpenDoor
    | ClosedDoor
    | Grass
    | Tree
    | SmallPlants
    | Bush
    | Glass
    | Sand
    | Water
    | StairsDown
    | StairsUp
    | MainMapForest
    | MainMapGrassland
    | MainMapWater
    | MainMapMountains
    | MainMapCoast

let obstacles = set [ Wall; ClosedDoor; Tree ]

type LevelType = 
    | Test
    | Dungeon
    | Cave
    | Forest
    | Empty
    | Grassland
    | Coast

type TransportTarget = {
    BoardId : Guid;
    TargetCoordinates : Point
}   

type Ore = 
    | NoneOre
    | Iron of Quantity
    | Gold of Quantity
    | Uranium of Quantity
    | CleanWater of Quantity
    | ContaminatedWater of Quantity 
    member this.Quantity 
        with get() = 
            match this with
            | Iron(value) -> value
            | Gold(value) -> value
            | Uranium(value) -> value
            | CleanWater(value) -> value
            | ContaminatedWater(value) -> value
            | _ -> QuantityValue(0)

type Place = {
    Tile : Tile; 
    Items : Item list;
    Ore : Ore
    Character : Character option;    
    IsSeen : bool;
    WasSeen : bool;
    TransportTarget : TransportTarget option
} with
    static member EmptyPlace = 
            {Tile = Tile.Empty; Items = []; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; Ore = NoneOre; TransportTarget = Option.None}
    static member Wall = 
            {Tile = Tile.Wall; Items = []; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; Ore = NoneOre; TransportTarget = Option.None }
    static member GetDescription (place: Place) =
        let tileDescription = 
            match place.Tile with
            | Tile.Floor -> "Floor."
            | Tile.Wall -> "Wall."
            | Tile.Glass -> "Glass."
            | Tile.Grass -> "Grass."
            | Tile.Bush -> "Some bushes."
            | Tile.SmallPlants -> "Some plants."
            | Tile.Tree -> "Tree."
            | Tile.Sand -> "Sand."
            | Tile.Water -> "Water."
            | Tile.ClosedDoor -> "Closed door."
            | Tile.OpenDoor -> "Open door."
            | Tile.StairsDown -> "Stairs leading down."
            | Tile.StairsUp -> "Stairs leading up."
            | _ -> ""
        let characterDescription =
            if place.Character.IsSome then
                " " + place.Character.Value.Name + " is standing here."
            else
                ""
        let itemsDescription =
            if place.Items.Length > 1 then
                " Some items are lying here."
            elif place.Items.Length > 0 then
                " " + place.Items.Head.Name + " is lying here."
            else
                ""
        tileDescription + characterDescription + itemsDescription

let boardHeight = 24
let boardWidth = 79

type Board = {
    Guid : System.Guid;
    Places : Place[,];
    Level : int;
    /// Defines the main map location which the current map is connected to.
    MainMapLocation: Point option
}
    
let boardContains (point: Point) = 
    boardWidth > point.X  && boardHeight > point.Y && point.X >= 0 && point.Y >= 0
                    
let get (board: Board) (point: Point) = if boardContains point then Array2D.get board.Places point.X point.Y else Place.Wall

let isMovementObstacle (board: Board) (point: Point) =
    ((get board point).Tile = Tile.Wall || (get board point).Tile = Tile.ClosedDoor || (get board point).Tile = Tile.Tree || (get board point).Tile = Tile.Glass || (get board point).Tile = Tile.MainMapWater || (get board point).Tile = Tile.MainMapMountains || (get board point).Character.IsSome)

let isOpticalObstacle (board: Board) (point: Point) =
    ((get board point).Tile = Tile.Wall || (get board point).Tile = Tile.ClosedDoor || (get board point).Tile = Tile.Tree || (get board point).Tile = Tile.Bush)

let set (point: Point) (value: Place) (board: Board) : Board =
    let result = Array2D.copy board.Places 
    Array2D.set result point.X point.Y value
    { board with Places = result }

let modify (point: Point) (modifier: Place -> Place) (board: Board) =
    let current = get board point 
    set point (modifier current) board

let places (board: Board) = 
    seq {
        for x = 0 to boardWidth - 1 do
            for y = 0 to boardHeight - 1 do
                let item = Array2D.get board.Places x y
                yield (new Point(x, y), item)
    }

let monsterPlaces (board: Board) = 
    board
    |> places
    |> Seq.choose (fun item -> 
        match (snd item).Character with 
        | Some(character) when (character :? Monster) -> Some(item) 
        | _ -> None)
    |> Seq.toList

let getPlayerPosition (board: Board) = 
    let preResult = Seq.tryFind (fun (point, place) -> 
        match place.Character with 
        | Some(character1) -> character1.Type = Avatar
        | _ -> false) (places board)
    let point, _ = preResult.Value
    point

let getPlayerCharacter (board: Board) =
    let preResult = Seq.tryFind (fun (point, place) -> 
        match place.Character with 
        | Some(character1) -> character1.Type = Avatar
        | _ -> false) (places board)
    let _ , place = preResult.Value
    place.Character.Value

let moveCharacter (character: Character) (newPosition: Point) (board: Board) =
    let allBoardPlaces = places board
    match Seq.tryFind (fun (_, place) -> 
        match place.Character with 
        | Some(character1) -> character1 = character
        | _ -> false) allBoardPlaces with        
    | Some((oldPosition, oldPlace)) when oldPosition <> newPosition ->           
        let character = (get board oldPosition ).Character
        board 
        |> modify newPosition (fun place -> { place with Character = character }) 
        |> modify oldPosition (fun place -> { place with Character = option.None }) 
    | _ ->
        board
        |> modify newPosition (fun place -> {place with Character = Some character })

let countObstaclesAroundPoint (point: Point) (board: Board) : int =
    let mutable count = 0
    for tmpx in (max 0 (point.X - 1))..(min (point.X + 1) (boardWidth - 1)) do
        for tmpy in (max 0 (point.Y - 1))..(min (point.Y + 1) (boardHeight - 1)) do
            if not(tmpx = point.X && tmpy = point.Y) then
                count <- count + (if(isMovementObstacle board (Point(tmpx,tmpy))) then 1 else 0)
    count


//let emptyBoard : Board = { Places = Array2D.create boardWidth boardHeight Place.EmptyPlace }

type IModifier =
    abstract member Modify: Board -> Board

type Room(rect: Rectangle) =
    let generateRoom width height = 
        if width < 1 then invalidArg "width" "Is zero"
        if height < 1 then invalidArg "height" "Is zero"

        let wall = {Place.EmptyPlace with Tile = Tile.Wall}
        let floor = {Place.EmptyPlace with Tile = Tile.Floor}
        let result = Array2D.create width height floor
        for x = 0 to width - 1 do 
            Array2D.set result x 0 wall
            Array2D.set result x (height - 1) wall
        for y = 0 to height - 1 do 
            Array2D.set result 0 y wall
            Array2D.set result (width - 1) y wall
        result

    member this.Rectangle
        with get() = rect

    override this.ToString() =
        rect.ToString()
    interface IModifier with
        member this.Modify board = 
            let room = generateRoom rect.Width rect.Height
            let width = Array2D.length1 room
            let height = Array2D.length2 room
            let result = board
            Array2D.blit room 0 0 result.Places rect.X rect.Y width height
            result

type Tunnel(rect: Rectangle, randomizeSize: bool) =
    
    let mutable actualTunnelRect = new Rectangle(0, 0, 0, 0)
    

    let generateTunnel width height = 
        if width < 1 then invalidArg "width" "Is zero"
        if height < 1 then invalidArg "height" "Is zero"

        let wall = {Place.EmptyPlace with Tile = Tile.Wall}
        let floor = {Place.EmptyPlace with Tile = Tile.Floor}

        let tunnelWidth = 
            if(randomizeSize) then rnd2 (min 2 width) width
            else width
        let tunnelHeight = 
            if(randomizeSize) then rnd2 (min 2 height) height
            else height
        let tunnelX = rnd(width - tunnelWidth)
        let tunnelY = rnd(height - tunnelHeight)

        actualTunnelRect <- new Rectangle(tunnelX + rect.X, tunnelY + rect.Y, tunnelWidth, tunnelHeight)

        let result = Array2D.create width height wall
        for x = tunnelX to tunnelX + tunnelWidth - 1 do 
            for y = tunnelY to tunnelY + tunnelHeight - 1 do 
                Array2D.set result x y floor
        result
    
    let generatedTunnel = generateTunnel rect.Width rect.Height

    
    member this.ActualTunnelRect = actualTunnelRect
    

    member this.GeneratedTunnel
        with get() = generatedTunnel

    member this.GetRandomPointInside
        with get() = (rnd2 actualTunnelRect.X (actualTunnelRect.X + actualTunnelRect.Width), rnd2 actualTunnelRect.Y (actualTunnelRect.Y + actualTunnelRect.Height))

    member this.Rectangle
        with get() = rect

    interface IModifier with
        member this.Modify board = 
            let tunnel = generatedTunnel
            let width = Array2D.length1 tunnel
            let height = Array2D.length2 tunnel
            let result = board
            Array2D.blit tunnel 0 0 result.Places rect.X rect.Y width height
            result

type private BoardMessage = 
| GetAt of Point * AsyncReplyChannel<Place>
| SetAt of Point * Place
| Apply of (Board -> Board)
| Set of Board

let private createProcessor board =
    MailboxProcessor<BoardMessage>.Start(fun inbox ->
        let rec loop (board: Board) = async {
            let! msg = inbox.Receive()
            match msg with 
            | GetAt(point, outbox) -> 
                outbox.Reply(Array2D.get board.Places point.X point.Y)
                return! loop board
            | SetAt(point, place) ->
                board.Places.[point.X, point.Y] <- place
                return! loop board
            | Apply(func) ->
                return! loop (func board)
            | Set(board) ->
                return! loop board
        }
        loop board)
        
// WARNING!!! The crap below... what is it for? what guid to put in here?
let private agent = createProcessor <| { Guid = Guid.NewGuid(); Places = Array2D.create boardWidth boardHeight Place.EmptyPlace; Level = 0; MainMapLocation = Option.None }

// TODO: refact functions to use BoardMessage structure

