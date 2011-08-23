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
open Player
open Monsters
open AI
open Turn
open Characters

let evaluateBoardFramePosition state = 
    let playerPosition = getPlayerPosition state.Board
    let frameView = new Rectangle(state.BoardFramePosition, boardFrameSize)
    let preResult =
        let x = inBoundary (playerPosition.X - (boardFrameSize.Width / 2)) 0 (boardWidth - boardFrameSize.Width)
        let y = inBoundary (playerPosition.Y - (boardFrameSize.Height / 2)) 0 (boardHeight - boardFrameSize.Height)
        point x y                
    { state with BoardFramePosition = preResult }

let showEquipment () =
    let refreshScreen = 
        let items = (State.get ()).Player.Items
        Screen.showEquipmentItemDialog {Items = items; CanSelect = false}

    let rec loop () =
        let key = System.Console.ReadKey(true).Key        
        match key with 
        | ConsoleKey.Escape -> ()
        | _ -> 
            refreshScreen
            // proof of concept for passage of time for non-board actions
            Turn.elapse 0.4M option.None
            loop ()
    refreshScreen
    loop ()

let showItems () =
    let refreshScreen = 
        Screen.showChooseItemDialog { State = (State.get ()); Filter = fun _ -> true }

    let rec loop () =
        let key = System.Console.ReadKey(true).Key        
        match key with 
        | ConsoleKey.Escape -> ()
        | _ -> 
            refreshScreen            
            loop ()
    refreshScreen
    loop ()


let mainLoop () =
    let rec loop printAll =                

        if not(State.get().Player.IsAlive) then
            ()
        else
            let consoleKeyInfo = if printAll then new ConsoleKeyInfo('5', ConsoleKey.NumPad5, false, false, false) else System.Console.ReadKey(true)
        
            let command = 
                match consoleKeyInfo with 
                | Keys [ConsoleKey.UpArrow; '8'] -> Up            
                | Keys [ConsoleKey.DownArrow; '2'] -> Down            
                | Keys [ConsoleKey.LeftArrow; '4'] -> Left            
                | Keys [ConsoleKey.RightArrow; '6'] -> Right
                | Key '7'  -> UpLeft
                | Key '9' -> UpRight
                | Key '1' -> DownLeft
                | Key '3' -> DownRight
                | Key '5' -> Wait
                | Key ',' -> Take
                | Key 'i' -> ShowItems
                | Key ConsoleKey.Escape -> Quit
                | Key 'o' -> OpenDoor
                | Key 'c' -> CloseDoor
                | Key 'e' -> ShowEquipment
                | Key 'm' -> ShowMessages
                | Key 'h' -> Harvest
                | Key 'W' -> Wear
                | Key 'T' -> TakeOff
                | _ -> Unknown                        
        
            match command with
            | Quit -> ()
            | Unknown -> loop false
            | Up | Down | Left | Right | UpLeft | UpRight | DownLeft | DownRight  ->
                State.get () 
                |> moveAvatar command
                |> Turn.next
                Screen.showBoard ()
                loop false
            | Wait ->
                State.get () |> Turn.next
                Screen.showBoard ()
                loop false
            | Take ->
                State.get () 
                |> Actions.performTakeAction
                |> Turn.next
                Screen.showBoard ()
                loop false
            | OpenDoor | CloseDoor ->
                State.get () 
                |> Actions.performCloseOpenAction command
                |> Turn.next
                Screen.showBoard ()
                loop false
            | Harvest -> 
                State.get () 
                |> Actions.performHarvest
                |> Turn.next
                Screen.showBoard ()
                loop false
            | ShowItems ->
                showItems ()
                Screen.showBoard ()
                loop false
            | ShowEquipment ->
                showEquipment ()
                Screen.showBoard ()
                loop false
            | ShowMessages ->
                Screen.showMessages ()
                loop false
            | Wear ->
                State.get ()
                |> wear
                |> Turn.next
                Screen.showBoard ()
                loop false
            | TakeOff ->
                State.get ()
                |> takeOff
                |> Turn.next
                Screen.showBoard ()
                loop false

    let mainMenuReply = showMainMenu ()

    let thePlayer = new Player(mainMenuReply.Name, 10)

    let board = 
        generateLevel LevelType.Cave
        |> Board.moveCharacter thePlayer (new Point(8, 4))

    let entryState = {         
        Board = board; 
        BoardFramePosition = point 0 0;
        Player = thePlayer
        TurnNumber = 0;
        UserMessages = [];
        Monsters = []
    }
    State.set entryState
    loop true      

[<EntryPoint>]
let main args =    
    mainLoop()
    //printf "%s" <| Seq.fold (fun acc x -> acc + " " + x.ToString()) "" (circles.[2])
    System.Console.ReadKey(true) |> ignore
    0