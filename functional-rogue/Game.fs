module Game

open System
open System.Drawing
open State
open Log
open Board
open LevelGeneration
open Screen
open Sight
open Items

type Command = 
    | Up
    | Down
    | Left
    | Right
    | Wait
    | Take
    | ShowItems
    | Quit
    | Unknown
    | OpenDoor
    | CloseDoor


let commandToSize command = 
    match command with
    | Up -> new Size(0, -1)
    | Down -> new Size(0, 1)
    | Left -> new Size(-1, 0)
    | Right -> new Size(1, 0)
    | _ -> new Size(0, 0)

let moveCharacter command state = 
    let board = state.Board
    let playerPosition = getPlayerPosition board

    let move = commandToSize command
    let newPosition = playerPosition + move
    let newPlace = get board newPosition
    
    let preResult = 
        match newPlace.Tile with 
        | Wall | ClosedDoor -> board
        | _ ->         
            board |> moveCharacter {Type = Avatar} newPosition
    {state with Board = preResult}

let operateDoor command board =
    let playerPosition = getPlayerPosition board
    let oldDoor = {Place.EmptyPlace with Tile = (if (command = OpenDoor) then Tile.ClosedDoor else Tile.OpenDoor)}
    let newDoor = {Place.EmptyPlace with Tile = (if (command = OpenDoor) then Tile.OpenDoor else Tile.ClosedDoor)}
    for x in (max 0 (playerPosition.X - 1))..(min boardWidth (playerPosition.X + 1)) do
        for y in (max 0 (playerPosition.Y - 1))..(min boardHeight (playerPosition.Y + 1)) do
            if(not(x = playerPosition.X && y = playerPosition.Y) && board.[x,y].Tile = oldDoor.Tile) then
                Array2D.set board x y newDoor
    board 

let performCloseOpenAction command state =
    match command with
    | OpenDoor | CloseDoor -> { state with Board = operateDoor command state.Board }
    | _ -> state

let performTakeAction command state = 
    let playerPosition = getPlayerPosition state.Board
    if command = Take then
        let place = get state.Board playerPosition
        let takenItems = place.Items
        let board1 = 
            state.Board
            |> set playerPosition {place with Items = []}
        let state1 = 
            let extractGold items =
                Seq.sumBy (function | Gold(value) -> value | _ -> 0) items
            let gold = state.Player.Gold + extractGold takenItems
            { state with Player = { state.Player with Items =  takenItems @ state.Player.Items; Gold = gold}}

        {state1 with Board = board1}
    else
        state

let evaluateBoardFramePosition state = 
    let playerPosition = getPlayerPosition state.Board
    let frameView = new Rectangle(state.BoardFramePosition, boardFrameSize)
    let preResult =                 
        let x = inBoundary (playerPosition.X - (boardFrameSize.Width / 2)) 0 (boardWidth - boardFrameSize.Width)
        let y = inBoundary (playerPosition.Y - (boardFrameSize.Height / 2)) 0 (boardHeight - boardFrameSize.Height)
        point x y                
    { state with BoardFramePosition = preResult }

let mainLoop() =
    let rec loop printAll =                
        let nextTurn command =             
            let state = 
                State.get ()
                |> moveCharacter command
                |> performCloseOpenAction command
                |> performTakeAction command
                |> setVisibilityStates
                |> evaluateBoardFramePosition
                                    
            State.set {state with TurnNumber = state.TurnNumber + 1}
            Screen.showBoard ()

        let key = if printAll then ConsoleKey.W else System.Console.ReadKey(true).Key
        
        let command = 
            match key with 
            | ConsoleKey.UpArrow -> Up            
            | ConsoleKey.DownArrow -> Down            
            | ConsoleKey.LeftArrow -> Left            
            | ConsoleKey.RightArrow -> Right
            | ConsoleKey.W -> Wait
            | ConsoleKey.OemComma -> Take
            | ConsoleKey.I -> ShowItems
            | ConsoleKey.Escape -> Quit
            | ConsoleKey.O -> OpenDoor
            | ConsoleKey.C -> CloseDoor
            | _ -> Unknown                        
        
        match command with
        | Quit -> ()
        | Unknown -> loop false
        | Up | Down | Left | Right | Wait | Take | OpenDoor | CloseDoor -> 
            (nextTurn command)                
            loop false
        | ShowItems ->
            showChooseItemDialog {Items = (State.get ()).Player.Items; CanSelect = false} |> ignore
            loop false

    let board = 
        generateLevel LevelType.Dungeon
        |> Board.moveCharacter {Type = CharacterType.Avatar} (new Point(8, 4))

    let mainMenuReply = showMainMenu ()
    let entryState = {         
        Board = board; 
        BoardFramePosition = point 0 0;
        Player = { Name = mainMenuReply.Name; HP = 5; MaxHP = 10; Magic = 5; MaxMagic = 10; Gold = 0; SightRadius = 10; Items = []};
        TurnNumber = 0;
    }
    State.set entryState
    loop true      

[<EntryPoint>]
let main args =    
    mainLoop()
    //printf "%s" <| Seq.fold (fun acc x -> acc + " " + x.ToString()) "" (circles.[2])
    System.Console.ReadKey(true) |> ignore
    0