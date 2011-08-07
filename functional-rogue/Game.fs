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

let moveCharacter character command board = 
    let position, _ =  Seq.find (fun (_, place) -> place.Character = Some character) <| places board

    let move = commandToSize command
    let newPosition = position + move
    let newPlace = get board newPosition
    
    match newPlace.Tile with 
    | Wall | ClosedDoor -> board
    | _ ->         
        board |> moveCharacter character newPosition

let operateDoor character command board =
    let mutable oldDoor = {Place.EmptyPlace with Tile = (if (command = OpenDoor) then Tile.ClosedDoor else Tile.OpenDoor)}
    let mutable newDoor = {Place.EmptyPlace with Tile = (if (command = OpenDoor) then Tile.OpenDoor else Tile.ClosedDoor)}
    let position, _ =  Seq.find (fun (_, place) -> place.Character = Some character) <| places board
    for x in (max 0 (position.X - 1))..(min boardWidth (position.X + 1)) do
        for y in (max 0 (position.Y - 1))..(min boardHeight (position.Y + 1)) do
            if(not(x = position.X && y = position.Y) && board.[x,y].Tile = oldDoor.Tile) then
                Array2D.set board x y newDoor
    board 

let performAction character command board =
    match command with
    | OpenDoor | CloseDoor -> operateDoor character command board
    | _ -> board

let mainLoop() =
    let rec loop printAll =                
        let nextTurn command = 
            let state = State.get ()
            let board = 
                state.Board  
                |> moveCharacter {Type = Avatar} command
                |> performAction {Type = Avatar} command
                |> setVisibilityStates state.Player
                    
            // evaluate BoardFramePosition
            let playerPosition = getPlayerPosition board
            let frameView = new Rectangle(state.BoardFramePosition, boardFrameSize)
            let boardFramePosition =                 
                let x = inBoundary (playerPosition.X - (boardFrameSize.Width / 2)) 0 (boardWidth - boardFrameSize.Width)
                let y = inBoundary (playerPosition.Y - (boardFrameSize.Height / 2)) 0 (boardHeight - boardFrameSize.Height)
                point x y                
            
            // take item
            let board1, state1 = 
                if command = Take then
                    let place = get board playerPosition
                    let takenItems = place.Items
                    let board1 = 
                        board
                        |> set playerPosition {place with Items = []}
                    let state1 = 
                        let extractGold items =
                            Seq.sumBy (function | Gold(value) -> value | _ -> 0) items
                        let gold = state.Player.Gold + extractGold takenItems
                        { state with Player = { state.Player with Items =  takenItems @ state.Player.Items; Gold = gold}}

                    board1, state1
                else
                    board, state
                

            State.set {state with Board = board; TurnNumber = state.TurnNumber + 1; BoardFramePosition = boardFramePosition}

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