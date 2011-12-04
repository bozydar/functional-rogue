﻿module Game

open System
open System.Drawing
open State
open Log
open Board
open LevelGeneration
open Screen
open Sight
open Actions
open Player
open Monsters
open AI
open Turn
open Characters
open System.IO

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
            State.get ()                  
            |> Screen.showFinishScreen
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
                | Key 'o' -> OpenCloseDoor
                | Key 'e' -> ShowEquipment
                | Key 'm' -> ShowMessages
                | Key 'h' -> Harvest
                | Key 'W' -> Wear
                | Key 'T' -> TakeOff
                | Key '>' -> GoDownEnter
                | Key '<' -> GoUp
                | Key 'l' -> Look
                | Key 'U' -> UseObject
                | Key 'O' -> ToggleSettingsMainMapHighlightPointsOfInterest
                | _ -> Unknown                        
        
            match command with
            | Quit -> 
                State.get ()
                |> writeState
                ()
            | Unknown -> loop false
            | Up | Down | Left | Right | UpLeft | UpRight | DownLeft | DownRight  ->
                State.get () 
                |> moveAvatar command
                |> Turn.next
                Screen.showBoard ()
                loop false
            | GoDownEnter ->
                State.get () 
                |> performGoDownEnterAction command
                |> Turn.next
                Screen.showBoard ()
                loop false
            | GoUp ->
                State.get () 
                |> performGoUpAction command
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
            | OpenCloseDoor ->
                State.get () 
                |> Actions.performCloseOpenAction command
                |> Turn.next
                Screen.showBoard ()
                loop false
            | ToggleSettingsMainMapHighlightPointsOfInterest ->
                State.set (State.get() |> Actions.performToggleSettingsMainMapHighlightPointsOfInterest command)
                Screen.showBoard ()
                loop false
            | Look ->
                Actions.performLookAction command (State.get ())
                Screen.showBoard ()
                loop false
            | UseObject ->
                State.get ()
                |> performUseObjectAction command
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
    

    let d1 : Dialog.Dialog = [
        Dialog.Title("Welcome to functional rogue" ) ;
        Dialog.Label("Do you want to play?");        
        Dialog.Menu("ynResult",
            [
                Dialog.Item('y', "Yes", "1");
                Dialog.Item('n', "No", "0");
            ])        
    ]

    let test = showDialog d1
    if test.Head = ("ynResult", "1") then

        // TODO: Replace mainMenuReply with wigdet from dialog
        let mainMenuReply = showMainMenu ()
        let thePlayer = new Player(mainMenuReply.Name, 20, 10, 16, 10)

        //initial maps setup
        let mainMapBoard, mainMapPoint = generateMainMap

        let startLevel, startLevelPosition = if Config.Settings.TestStartOnEmpty then generateTestStartLocationWithInitialPlayerPositon mainMapPoint else generateStartLocationWithInitialPlayerPositon mainMapPoint
        let board = startLevel |> Board.moveCharacter thePlayer (startLevelPosition)

        let mainBoardStartPlace = mainMapBoard.Places.[mainMapPoint.X,mainMapPoint.Y]
        mainMapBoard.Places.[mainMapPoint.X,mainMapPoint.Y] <- { mainBoardStartPlace with TransportTarget = Some({ BoardId = board.Guid; TargetCoordinates = startLevelPosition; TargetLevelType = LevelType.Forest})}
    
        let initialBoards = new System.Collections.Generic.Dictionary<System.Guid,Board>()
        initialBoards.Add(board.Guid, board)
        initialBoards.Add(mainMapBoard.Guid, mainMapBoard)
        //end maps setup

        let getInitialReplicationRecipes = 
            let result = new System.Collections.Generic.HashSet<string>()
            ignore (result.Add("Knife"))
            result

        let entryState =
            try
                if Config.Settings.LoadSave then loadState () else raise (new FileNotFoundException())
            with
                | :? FileNotFoundException ->
                        {         
                            Board = board; 
                            BoardFramePosition = point 0 0;
                            Player = thePlayer
                            TurnNumber = 0;
                            UserMessages = [];
                            AllBoards = initialBoards;
                            MainMapGuid = mainMapBoard.Guid;
                        AvailableReplicationRecipes = getInitialReplicationRecipes;
                        MainMapDetails = Array2D.init boardWidth boardHeight (fun x y -> if mainMapPoint.X = x && mainMapPoint.Y = y then { PointOfInterest = Some("Your ship's crash site")} else { PointOfInterest = Option.None});
                        Settings = { HighlightPointsOfInterest = false }
                        }
        State.set entryState
        loop true      

let clearCharacterStates state = 
    let characters = 
        Board.getFilteredPlaces (fun item -> item.Character.IsSome) state.Board
        |> List.map (fun (_, item) -> item.Character.Value)
    characters |> List.iter (fun item -> item.ResetVolatileStates())
    state

let subscribeHandlers () =
    Turn.subscribe handleMonsters
    Turn.subscribe clearCharacterStates
    Turn.subscribe setVisibilityStates
    Turn.subscribe evaluateBoardFramePosition
    Turn.subscribe (fun state -> {state with TurnNumber = state.TurnNumber + 1})

[<EntryPoint>]
let main args =    
    subscribeHandlers ()
    mainLoop ()    
    0