module Actions

open System
open System.Drawing
open State
open Log
open Board
open LevelGeneration
open Screen
open Sight
open Items
open Player
open Characters

type Command = 
    | Up
    | Down
    | Left
    | Right
    | UpLeft
    | UpRight
    | DownLeft
    | DownRight
    | Wait
    | Take
    | ShowItems
    | Quit
    | Unknown
    | OpenCloseDoor
    | ShowEquipment
    | ShowMessages
    | Harvest
    | Wear
    | TakeOff
    | GoDownEnter
    | GoUp
    | Look

let private commandToSize command = 
    match command with
    | Up -> new Size(0, -1)
    | Down -> new Size(0, 1)
    | Left -> new Size(-1, 0)
    | Right -> new Size(1, 0)
    | UpLeft -> new Size(-1, -1)
    | UpRight -> new Size(1, -1)
    | DownLeft -> new Size(-1, 1)
    | DownRight -> new Size(1, 1)
    | _ -> new Size(0, 0)

let switchToTheMainMapBoard (oldBoard: Board) (playerPoint: Point) (state: State) =
    let playerPlace = oldBoard.Places.[playerPoint.X,playerPoint.Y]
    let currentPlayer = getPlayerCharacter oldBoard
    let newBoard = state.AllBoards.[state.MainMapGuid]
    let startPlace = newBoard.Places.[oldBoard.MainMapLocation.Value.X,oldBoard.MainMapLocation.Value.Y]
    newBoard.Places.[oldBoard.MainMapLocation.Value.X,oldBoard.MainMapLocation.Value.Y] <- { startPlace with Character = Some(currentPlayer) }
    oldBoard.Places.[playerPoint.X,playerPoint.Y] <- { oldBoard.Places.[playerPoint.X,playerPoint.Y] with Character = Option.None }
    state.AllBoards.[oldBoard.Guid] <- oldBoard
    { state with Board = newBoard }

let moveAvatar command state = 
    let board = state.Board
    let playerPosition = getPlayerPosition board
    let move = commandToSize command
    let newPosition = playerPosition + move
    if (newPosition.X < 0 || newPosition.X >= boardWidth || newPosition.Y < 0 || newPosition.Y >= boardHeight) then
        switchToTheMainMapBoard board playerPosition state
    else
        let newPlace = get board newPosition
        let playerCharacter = getPlayerCharacter board
    
        if canAttack board newPosition then
            Mechanics.meleeAttack playerCharacter (get board newPosition).Character.Value state
        elif isMovementObstacle state.Board newPosition then
            state
        else
            { state with Board = state.Board |> moveCharacter playerCharacter newPosition }

let selectPlace (positions : Point list) state : Point option =
    if positions.Length <> 0 then 
        let start = positions.Head
        let left = 
            fun (position : Point) -> 
                positions
                |> List.filter (fun item -> item.X < position.X)  
                |> List.sortBy (fun item -> -item.X, Math.Abs (position.Y - item.Y))
                |> fun x -> x @ [position]
                |> List.head
        
        let right = 
            fun (position : Point) -> 
                positions
                |> List.filter (fun item -> item.X > position.X)  
                |> List.sortBy (fun item -> item.X, Math.Abs (position.Y - item.Y)) 
                |> fun x -> x @ [position]
                |> List.head

        let up = 
            fun (position : Point) -> 
                positions
                |> List.filter (fun item -> item.Y < position.Y)  
                |> List.sortBy (fun item -> -item.Y, Math.Abs (position.X - item.X))
                |> fun x -> x @ [position]
                |> List.head

        let down = 
            fun (position : Point) -> 
                positions
                |> List.filter (fun item -> item.Y > position.Y)  
                |> List.sortBy (fun item -> item.Y, Math.Abs (position.X - item.X))
                |> fun x -> x @ [position]
                |> List.head

        let rec loop (current : Point) : Point option =
            setCursorPositionOnBoard current state
            let keyInfo = System.Console.ReadKey(true)
            match keyInfo with 
            | Keys [ConsoleKey.UpArrow; '8'] -> loop (up current) //Up
            | Keys [ConsoleKey.DownArrow; '2'] -> loop (down current) //Down        
            | Keys [ConsoleKey.LeftArrow; '4'] -> loop (left current) //Left            
            | Keys [ConsoleKey.RightArrow; '6'] -> loop (right current) //Right
            | Key ConsoleKey.Enter -> Some(current)
            | Key ConsoleKey.Escape -> None
            | _ -> loop current
        loop start                
    else
        None

let private operateDoor command state =
    let board = state.Board
    let playerPosition = getPlayerPosition board
    let points = 
        playerPosition 
        :: [for x in (max 0 (playerPosition.X - 1))..(min boardWidth (playerPosition.X + 1)) do
                for y in (max 0 (playerPosition.Y - 1))..(min boardHeight (playerPosition.Y + 1)) do
                    let p = Point(x, y)
                    if p <> playerPosition then yield p]        
    let selected = selectPlace points state

    if selected.IsSome then 
        board |> Board.modify selected.Value (
            fun (place : Place) -> 
                match place.Tile with
                | Tile.OpenDoor -> {place with Tile = Tile.ClosedDoor}  
                | Tile.ClosedDoor -> {place with Tile = Tile.OpenDoor}
                | _ -> place)
    else
        board

let performCloseOpenAction command state =
    { state with Board = operateDoor command state } 

let performLookAction command state =
    let board = state.Board
    let playerPosition = getPlayerPosition board
    let points = visiblePositions playerPosition state.Player.SightRadius board    
    ignore (selectPlace points state)

let switchBoards (oldBoard: Board) (playerPoint: Point) (state: State) =
    let playerPlace = oldBoard.Places.[playerPoint.X,playerPoint.Y]
    let currentPlayer = getPlayerCharacter oldBoard
    let newBoard = state.AllBoards.[playerPlace.TransportTarget.Value.BoardId]
    let startPlace = newBoard.Places.[playerPlace.TransportTarget.Value.TargetCoordinates.X, playerPlace.TransportTarget.Value.TargetCoordinates.Y]
    newBoard.Places.[playerPlace.TransportTarget.Value.TargetCoordinates.X, playerPlace.TransportTarget.Value.TargetCoordinates.Y] <- { startPlace with Character = Some(currentPlayer) }
    oldBoard.Places.[playerPoint.X,playerPoint.Y] <- { oldBoard.Places.[playerPoint.X,playerPoint.Y] with Character = Option.None }
    state.AllBoards.[oldBoard.Guid] <- oldBoard
    { state with Board = newBoard }
    
let performGoDownEnterAction (command: Command) state =
    let currentBoard = state.Board  
    let playerPosition = getPlayerPosition currentBoard
    let currentPlayer = getPlayerCharacter currentBoard
    let playerPlace = currentBoard.Places.[playerPosition.X,playerPosition.Y]
    if (playerPlace.Tile = Tile.StairsDown || playerPlace.Tile = Tile.MainMapForest || playerPlace.Tile = Tile.MainMapGrassland || playerPlace.Tile = Tile.MainMapCoast) then
        if (playerPlace.TransportTarget.IsNone) then
            let targetMapType = 
                match playerPlace.Tile with
                | Tile.MainMapForest -> LevelType.Forest
                | Tile.MainMapGrassland -> LevelType.Grassland
                | Tile.MainMapCoast -> LevelType.Coast
                | _ -> LevelType.Cave
            let newBoard, newPoint = generateLevel targetMapType (Some({BoardId = currentBoard.Guid; TargetCoordinates = playerPosition})) (Some(currentBoard.Level - 1))
            state.AllBoards.Add(newBoard.Guid, newBoard)                    
            currentBoard.Places.[playerPosition.X,playerPosition.Y] <- {playerPlace with TransportTarget = Some({ BoardId = newBoard.Guid; TargetCoordinates = newPoint.Value }) }
        switchBoards currentBoard playerPosition state
    else
        state |> addMessage (sprintf "There are no stairs down nor entrance here.")

let performGoUpAction (command: Command) state =
    let currentBoard = state.Board  
    let playerPosition = getPlayerPosition currentBoard
    let currentPlayer = getPlayerCharacter currentBoard
    let playerPlace = currentBoard.Places.[playerPosition.X,playerPosition.Y]
    if (playerPlace.Tile = Tile.StairsUp) then
        if (playerPlace.TransportTarget.IsNone) then
            let newBoard, newPoint = generateLevel LevelType.Cave (Some({BoardId = currentBoard.Guid; TargetCoordinates = playerPosition})) (Some(currentBoard.Level + 1))
            state.AllBoards.Add(newBoard.Guid, newBoard)
        switchBoards currentBoard playerPosition state
    else
        state |> addMessage (sprintf "There are no stairs up nor exit here.")

let performTakeAction state =     
    let playerPosition = getPlayerPosition state.Board
    let place = get state.Board playerPosition
    let takenItems = place.Items
    let pickUpMessages = List.map (fun i -> (sprintf "You have picked up an item: %s" (itemShortDescription i))) takenItems
    let board1 = 
        state.Board
        |> set playerPosition {place with Items = []}
    let state1 = 
        let shortCuts = createShortCuts state.Player.ShortCuts takenItems
            
        //{ state with Player = { state.Player with Items =  takenItems @ state.Player.Items; ShortCuts = shortCuts }}
        state.Player.Items <- takenItems @ state.Player.Items
        state.Player.ShortCuts <- shortCuts
        state
        |> addMessages pickUpMessages

    {state1 with Board = board1}

let performHarvest state = 
    let playerPosition = getPlayerPosition state.Board
    let place = get state.Board playerPosition
    let harvestRate =
        match place.Ore with
        | Iron(v) | Gold(v) | Uranium(v) ->
            if state.Player.WornItems.Hand.IsSome then
                state.Player.WornItems.Hand.Value.MiscProperties.OreExtractionRate
            else
                0
        | _ -> 5    //rate for water
    if harvestRate = 0 then
        state |> addMessage "You cannot harvest anything. You need a special tool for that." 
    else
        let takenOre, leftOre = 
            if place.Ore.Quantity.IsInf || place.Ore.Quantity.Value > harvestRate then
                match place.Ore with
                | Iron(v) -> Iron(Quantity.QuantityValue(harvestRate)),Iron(Quantity.QuantityValue(v.Value - harvestRate))
                | Gold(v) -> Gold(Quantity.QuantityValue(harvestRate)),Gold(Quantity.QuantityValue(v.Value - harvestRate))
                | Uranium(v) -> Uranium(Quantity.QuantityValue(harvestRate)),Uranium(Quantity.QuantityValue(v.Value - harvestRate))
                | CleanWater(v) -> CleanWater(Quantity.QuantityValue(harvestRate)),CleanWater(Quantity.PositiveInfinity)
                | ContaminatedWater(v) -> ContaminatedWater(Quantity.QuantityValue(harvestRate)),ContaminatedWater(Quantity.PositiveInfinity)
                | _ -> Ore.NoneOre, place.Ore
            else
                place.Ore, Ore.NoneOre
        if takenOre.Quantity.IsInf then raise <| new NotImplementedException("Here should be asking for not inf")
        match takenOre with
        | Iron(quantity) -> state.Player.Iron <- state.Player.Iron + quantity.Value
        | Gold(quantity) -> state.Player.Gold <- state.Player.Gold + quantity.Value
        | Uranium(quantity) -> state.Player.Uranium <- state.Player.Uranium + quantity.Value
        | CleanWater(quantity) -> state.Player.Water <- state.Player.Water + quantity.Value
        | ContaminatedWater(quantity) -> state.Player.ContaminatedWater <- state.Player.ContaminatedWater + quantity.Value
        | _ -> ()
        match takenOre with
            | Iron(quantity) | Gold(quantity) | Uranium(quantity) | CleanWater(quantity) | ContaminatedWater(quantity) ->
                let pickUpMessage = sprintf "You have harvested ore %s. Quantity: %s" (repr takenOre) (quantity.ToString())
                let board1 = 
                    state.Board
                    |> fun board -> if not quantity.IsInf then set playerPosition {place with Ore = leftOre} board else board
                {state with Board = board1} |> addMessage pickUpMessage                
            | NoneOre -> 
                state

let chooseItem () =
    let refreshScreen = 
        Screen.showChooseItemDialog {State = (State.get ()); Filter = fun _ -> true}

    let rec loop () =
        let keyInfo = System.Console.ReadKey(true)
        match keyInfo with 
        | Key ConsoleKey.Escape -> ()
        | _ -> 
            refreshScreen            
            loop ()
    refreshScreen
    loop ()

let chooseOption (options : list<char * string>)  =
    let refreshScreen =         
        Screen.showOptions options
    
    let keys = options |> List.map (fun (key, _) -> key :> obj) 

    let rec loop () =
        let keyInfo = System.Console.ReadKey(true)
        match keyInfo with 
        | Keys keys -> keyInfo.KeyChar
        | _ -> 
            refreshScreen            
            loop ()
    refreshScreen
    loop ()
    


let wear (state : State) = 
    let alreadyWorn =     
        [state.Player.WornItems.Hand; state.Player.WornItems.Head; state.Player.WornItems.Legs; state.Player.WornItems.Torso]
        |> List.filter Option.isSome
        |> List.map Option.get

    let refreshScreen = 
        Screen.showChooseItemDialog {State = state; Filter = (fun item -> not <| List.exists ((=) item) alreadyWorn)}

    let rec loop () =
        let keyInfo = System.Console.ReadKey(true)
        match keyInfo with 
        | Key ConsoleKey.Escape -> state
        | _ -> 
            let keyChar = keyInfo.KeyChar
            let item = Map.tryGetItem keyChar state.Player.ShortCuts 
            if item.IsSome && not <| List.exists ((=) item.Value) alreadyWorn then
                let result = 
                    let options = Seq.toList <| seq {
                        if item.Value.Wearing.InHand then yield ('g', "Grab")
                        if item.Value.Wearing.OnHead then yield ('h', "Put on head")
                        if item.Value.Wearing.OnLegs then yield ('l', "Put on legs")
                        if item.Value.Wearing.OnTorso then yield ('t', "Put on torso")
                    }
                    let chosenOption = chooseOption options                           
                        
                    match chosenOption with
                    | 'g' -> state.Player.WornItems <- {state.Player.WornItems with Hand = item }
                    | 'h' -> state.Player.WornItems <- {state.Player.WornItems with Head = item }
                    | 'l' -> state.Player.WornItems <- {state.Player.WornItems with Legs = item }
                    | 't' -> state.Player.WornItems <- {state.Player.WornItems with Torso = item }                        
                    | _ -> ()

                state
            else
                refreshScreen 
                loop ()
    refreshScreen
    loop ()

let takeOff (state : State) = 
    let options = Seq.toList <| seq {
        if state.Player.WornItems.Hand.IsSome then yield ('g', "Put off - " + itemShortDescription state.Player.WornItems.Hand.Value)
        if state.Player.WornItems.Head.IsSome then yield ('h', "Take off from head - " + itemShortDescription state.Player.WornItems.Head.Value)
        if state.Player.WornItems.Legs.IsSome then yield ('l', "Remove from legs - " + itemShortDescription state.Player.WornItems.Legs.Value)
        if state.Player.WornItems.Torso.IsSome then yield ('t', "Take off from torso - " + itemShortDescription state.Player.WornItems.Torso.Value)
    }
    let chosenOption = 
        if options.Length > 0  then 
            chooseOption options                           
        else
            ' '                    
    match chosenOption with
    | 'g' -> state.Player.WornItems <- {state.Player.WornItems with Hand = None }
    | 'h' -> state.Player.WornItems <- {state.Player.WornItems with Head = None }
    | 'l' -> state.Player.WornItems <- {state.Player.WornItems with Legs = None }
    | 't' -> state.Player.WornItems <- {state.Player.WornItems with Torso = None }                      
    | _ -> ()
    state

