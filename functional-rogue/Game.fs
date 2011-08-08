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
open Actions


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