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
    | OpenDoor
    | CloseDoor
    | ShowEquipment
    | ShowMessages
    | Harvest
    | Wear
    | TakeOff
    | GoDownEnter
    | GoUp

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
    
        let preResult =
            if (isMovementObstacle board newPosition) then
                board
            else
                board |> moveCharacter playerCharacter newPosition
        {state with Board = preResult}

let private operateDoor command board =
    let playerPosition = getPlayerPosition board
    let oldDoor = {Place.EmptyPlace with Tile = (if (command = OpenDoor) then Tile.ClosedDoor else Tile.OpenDoor)}
    let newDoor = {Place.EmptyPlace with Tile = (if (command = OpenDoor) then Tile.OpenDoor else Tile.ClosedDoor)}
    for x in (max 0 (playerPosition.X - 1))..(min boardWidth (playerPosition.X + 1)) do
        for y in (max 0 (playerPosition.Y - 1))..(min boardHeight (playerPosition.Y + 1)) do
            if(not(x = playerPosition.X && y = playerPosition.Y) && board.Places.[x,y].Tile = oldDoor.Tile) then
                Array2D.set board.Places x y newDoor
    board 

let performCloseOpenAction command state =
    { state with Board = operateDoor command state.Board } 

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
    if (playerPlace.Tile = Tile.StairsDown || playerPlace.Tile = Tile.MainMapForest || playerPlace.Tile = Tile.MainMapGrassland) then
        if (playerPlace.TransportTarget.IsNone) then
            let targetMapType = 
                match playerPlace.Tile with
                | Tile.MainMapForest | Tile.MainMapGrassland -> LevelType.Forest
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
    let takenOre = place.Ore
    match takenOre with
    | Iron(quantity) -> state.Player.Iron <- state.Player.Iron + quantity
    | Gold(quantity) -> state.Player.Gold <- state.Player.Gold + quantity
    | Uranium(quantity) -> state.Player.Uranium <- state.Player.Uranium + quantity
    | _ -> ()
    match takenOre with
        | Iron(quantity) | Gold(quantity) | Uranium(quantity) ->
            let pickUpMessage = sprintf "You have harvested ore %s. Quantity: %i" (repr takenOre) quantity
            let board1 = 
                state.Board
                |> set playerPosition {place with Ore = NoneOre}
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

