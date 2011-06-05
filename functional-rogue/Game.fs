module Game

open System
open System.Drawing
open Log
open Board
open LevelGeneration

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

type State = {
    Board: Board
}


let mainLoop() =
    let rec loop printAll (states: State list) =                
        let nextTurn command = 
            let last = states.Head
            // Something... Something...
            //let avatar = last.Avatar.Move command
            let board = 
                last.Board  
                |> moveCharacter {Type = Avatar} command
            {Board = board}

        states.Head.Board 
        |> if printAll then printBoard Board.emptyBoard else printBoard states.[1].Board

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
        | Unknown -> loop false states
        | Up | Down | Left | Right | Wait -> loop false ((nextTurn command) :: states)

    let board = 
        generateLevel 
        |> Board.moveCharacter {Type = CharacterType.Avatar} (new Point(1, 1))

    let entryState = {         
        Board = board; 
    }
    loop true [entryState]

        
[<EntryPoint>]
let main args =    
    mainLoop()
    0
