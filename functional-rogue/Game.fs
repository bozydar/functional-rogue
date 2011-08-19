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
        let player = (State.get ()).Player
        Screen.showChooseItemDialog player

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

        let consoleKeyInfo = if printAll then new ConsoleKeyInfo('5', ConsoleKey.NumPad5, false, false, false) else System.Console.ReadKey(true)
        
        let command = 
            match consoleKeyInfo.Key, consoleKeyInfo.KeyChar with 
            | ConsoleKey.UpArrow, _ | _, '8' -> Up            
            | ConsoleKey.DownArrow, _ | _, '2' -> Down            
            | ConsoleKey.LeftArrow, _ | _, '4' -> Left            
            | ConsoleKey.RightArrow, _ | _, '6' -> Right
            | _, '7'  -> UpLeft
            | _, '9' -> UpRight
            | _, '1' -> DownLeft
            | _, '3' -> DownRight
            | _, '5' -> Wait
            | _, ',' -> Take
            | _, 'i' -> ShowItems
            | ConsoleKey.Escape, _ -> Quit
            | _, 'o' -> OpenDoor
            | _, 'c' -> CloseDoor
            | _, 'e' -> ShowEquipment
            | _, 'm' -> ShowMessages
            | _, 'h' -> Harvest
            | _ -> Unknown                        
        
        match command with
        | Quit -> ()
        | Unknown -> loop false
        | Up | Down | Left | Right | UpLeft | UpRight | DownLeft | DownRight  ->
            State.get () 
            |> moveCharacter command
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

    let board = 
        generateLevel LevelType.Cave
        |> Board.moveCharacter {Type = CharacterType.Avatar; Monster = Option.None} (new Point(8, 4))

    let mainMenuReply = showMainMenu ()
    let entryState = {         
        Board = board; 
        BoardFramePosition = point 0 0;
        Player = { 
                    Name = mainMenuReply.Name; 
                    HP = 5; MaxHP = 10; 
                    Magic = 5; 
                    MaxMagic = 10; 
                    Gold = 0; 
                    Iron = 0; 
                    Uranium = 0; 
                    SightRadius = 10; 
                    Items = []; 
                    WornItems = { Head = None; LeftHand = None; RightHand = None; Torso = None; Legs = None};
                    ShortCuts = Map []
                 };
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