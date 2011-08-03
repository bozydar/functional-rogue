module Game

open System
open System.Drawing
open State
open Log
open Board
open LevelGeneration
open Screen
open Sight

type Command = 
    | Up
    | Down
    | Left
    | Right
    | Wait
    | Quit
    | Unknown

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
    | Wall -> board
    | _ ->         
        board |> moveCharacter character newPosition
    
let mainLoop() =
    let rec loop printAll =                
        let nextTurn command = 
            let state = State.get ()
            let board = 
                state.Board  
                |> moveCharacter {Type = Avatar} command
                |> setVisibilityStates state.Player
                    
            // evaluate BoardFramePosition
            let playerPosition = getPlayerPosition board
            let frameView = new Rectangle(state.BoardFramePosition, boardFrameSize)
            let playerHandyArea = Rectangle.Inflate(frameView, -2, -2)
            let boardFramePosition = 
                if not <| playerHandyArea.Contains playerPosition then 
                    let x = inBoundary (playerPosition.X - (boardFrameSize.Width / 2)) 0 (boardWidth - boardFrameSize.Width)
                    let y = inBoundary (playerPosition.Y - (boardFrameSize.Height / 2)) 0 (boardHeight - boardFrameSize.Height)
                    point x y
                else
                    state.BoardFramePosition
            
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
            | ConsoleKey.Escape -> Quit
            | _ -> Unknown                        
        
        match command with
        | Quit -> ()
        | Unknown -> loop false
        | Up | Down | Left | Right | Wait -> 
            (nextTurn command)                
            loop false

    let board = 
        generateLevel 
        |> Board.moveCharacter {Type = CharacterType.Avatar} (new Point(1, 1))

    let mainMenuReply = showMainMenu ()
    let entryState = {         
        Board = board; 
        BoardFramePosition = point 0 0;
        Player = { Name = mainMenuReply.Name; HP = 5; MaxHP = 10; Magic = 5; MaxMagic = 10; Gold = 0; SightRadius = 10};
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