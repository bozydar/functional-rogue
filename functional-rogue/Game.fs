module Game

open System
open System.Drawing
open Log
open Board
open LevelGeneration
open ScreenActor

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
    Board: Board;
}


let mainLoop() =
    let rec loop printAll (state: State) =                
        let nextTurn command = 
            // Something... Something...
            //let avatar = last.Avatar.Move command
            let board = 
                state.Board  
                |> moveCharacter {Type = Avatar} command

            ScreenActor.refreshScreen {Board = board; BoardFramePosition = point 0 0; Statistics = "Nothing"}
            {Board = board}

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
        | Unknown -> loop false state
        | Up | Down | Left | Right | Wait -> loop false (nextTurn command)

    let board = 
        generateLevel 
        |> Board.moveCharacter {Type = CharacterType.Avatar} (new Point(1, 1))

    let entryState = {         
        Board = board; 
    }
    loop true entryState

     

[<EntryPoint>]
let main args =    
    mainLoop()
    0