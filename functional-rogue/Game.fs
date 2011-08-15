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

let mainLoop() =
    let rec loop printAll =                
        let nextTurn command =             
            Turn.next command
            Screen.showBoard ()

        let key = if printAll then ConsoleKey.W else System.Console.ReadKey(true).Key
        
        let command = 
            match key with 
            | ConsoleKey.UpArrow | ConsoleKey.NumPad8 -> Up            
            | ConsoleKey.DownArrow | ConsoleKey.NumPad2 -> Down            
            | ConsoleKey.LeftArrow | ConsoleKey.NumPad4 -> Left            
            | ConsoleKey.RightArrow | ConsoleKey.NumPad6 -> Right
            | ConsoleKey.NumPad7 -> UpLeft
            | ConsoleKey.NumPad9 -> UpRight
            | ConsoleKey.NumPad1 -> DownLeft
            | ConsoleKey.NumPad3 -> DownRight
            | ConsoleKey.W | ConsoleKey.NumPad5 -> Wait
            | ConsoleKey.OemComma -> Take
            | ConsoleKey.I -> ShowItems
            | ConsoleKey.Escape -> Quit
            | ConsoleKey.O -> OpenDoor
            | ConsoleKey.C -> CloseDoor
            | ConsoleKey.E -> ShowEquipment
            | ConsoleKey.M -> ShowMessages
            | _ -> Unknown                        
        
        match command with
        | Quit -> ()
        | Unknown -> loop false
        | Up | Down | Left | Right | UpLeft | UpRight | DownLeft | DownRight | Wait | Take | OpenDoor | CloseDoor -> 
            (nextTurn command)                
            loop false
        | ShowItems ->
            showChooseItemDialog {Items = (State.get ()).Player.Items; CanSelect = false; Filter = (fun x -> true)} |> ignore
            loop false
        | ShowEquipment ->
            showEquipmentItemDialog {Items = (State.get ()).Player.Items; CanSelect = false} |> ignore
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
        Player = { Name = mainMenuReply.Name; HP = 5; MaxHP = 10; Magic = 5; MaxMagic = 10; Gold = 0; SightRadius = 10; Items = []; WornItems = { Head = 0; InLeftHand = 0; InRightHand = 0} };
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