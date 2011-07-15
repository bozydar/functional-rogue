module Game

open System
open System.Drawing
open State
open Log
open Board
open LevelGeneration
open Screen

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
            // Something... Something...
            //let avatar = last.Avatar.Move command
            let board = 
                state.Board  
                |> moveCharacter {Type = Avatar} command
            State.set {Board = board}

            Screen.refresh {BoardFramePosition = point 0 0; Statistics = {HP = 10; MaxHP = 10; Magic = 10; MaxMagic = 10; Gold = 0}}

        let char = System.Console.ReadKey(true)        
        
        let command = 
            match char.Key with 
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

    let entryState = {         
        Board = board; 
    }
    State.set entryState
    loop true 

     

[<EntryPoint>]
let main args =    
    mainLoop()
    0