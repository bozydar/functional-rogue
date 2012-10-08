namespace FunctionalRogue

module Game =

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
            let key = Screen.readKey().Key        
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
        Screen.showChooseItemDialog { State = (State.get ()); Filter = fun _ -> true }

    exception QuitException
    
    let makeAction (consoleKeyInfo : ConsoleKeyInfo) =                

        if not(State.get().Player.IsAlive) then
            State.get ()                  
            |> Screen.showFinishScreen
            ()
        else
            let isMainMap = (State.get ()).Board.IsMainMap  
            let isCtrl = (consoleKeyInfo.Modifiers &&& ConsoleModifiers.Control) = (ConsoleModifiers.Control)
            let isShift = (consoleKeyInfo.Modifiers &&& ConsoleModifiers.Control) = (ConsoleModifiers.Shift)
            let boolTrue (value : bool) =
                value                  
            let command = 
                match consoleKeyInfo with 
                | Key ConsoleKey.K -> GoDownEnter
                | Key ConsoleKey.L -> GoUp
                | Keys [ConsoleKey.UpArrow; '8'] -> Up            
                | Keys [ConsoleKey.DownArrow; '2'] -> Down            
                | Keys [ConsoleKey.LeftArrow; '4'] -> Left            
                | Keys [ConsoleKey.RightArrow; '6'] -> Right
                | Key ConsoleKey.NumPad7  -> UpLeft
                | Key ConsoleKey.NumPad9 -> UpRight
                | Key ConsoleKey.NumPad1 -> DownLeft
                | Key ConsoleKey.NumPad3 -> DownRight
                | Key ConsoleKey.NumPad5 -> Wait
                | Key ConsoleKey.G when not isMainMap -> Take
                | Key ConsoleKey.I -> ShowItems
                | Key ConsoleKey.Escape -> Quit
                | Key ConsoleKey.O when not isMainMap -> OpenCloseDoor
                | Key ConsoleKey.E -> ShowEquipment
                | Key ConsoleKey.E when isShift -> Eat
                | Key ConsoleKey.M -> ShowMessages
                | Key ConsoleKey.H when not isMainMap -> Harvest
                | Key ConsoleKey.W -> Wear
                | Key ConsoleKey.T -> TakeOff
                | Key 'd' -> Drop
                | Key 'l' when not isMainMap -> Look
                | Key 'U' when not isMainMap -> UseObject   // objects are anything not in your inventory
                | Key 'u' -> UseItem    // items are things in your inventory
                | Key 'O' -> ToggleSettingsMainMapHighlightPointsOfInterest
                | Key ConsoleKey.P when isCtrl -> PourLiquid
                | Key '?' -> Help
                | Key 't' -> Throw
                | _ -> Unknown                        
        
            match command with
            | Quit -> 
                State.get ()
                |> writeState
                raise (QuitException)
            | Unknown -> ()
            | Up | Down | Left | Right | UpLeft | UpRight | DownLeft | DownRight  ->
                State.get () 
                |> moveAvatar command
                |> Turn.next
                Screen.showBoard ()
            | GoDownEnter ->
                State.get () 
                |> performGoDownEnterAction command
                |> Turn.next
                Screen.showBoard ()
            | GoUp ->
                State.get () 
                |> performGoUpAction command
                |> Turn.next
                Screen.showBoard ()
            | Wait ->
                State.get () |> Turn.next
                Screen.showBoard ()
            | Take ->
                State.get () 
                |> Actions.performTakeAction
                |> Turn.next
                Screen.showBoard ()
            | Drop ->
                State.get () 
                |> Actions.performDropAction
                |> Turn.next
                Screen.showBoard ()
            | Throw ->
                let oldState = State.get ()
                let newState, animationFunction = oldState |> Actions.performThrowAction
                if animationFunction.IsSome then Screen.showAnimation animationFunction.Value
                newState |> Turn.next
            | OpenCloseDoor ->
                State.get () 
                |> Actions.performCloseOpenAction command
                |> Turn.next
                Screen.showBoard ()
            | ToggleSettingsMainMapHighlightPointsOfInterest ->
                State.set (State.get() |> Actions.performToggleSettingsMainMapHighlightPointsOfInterest command)
                Screen.showBoard ()
            | Look ->
                Actions.performLookAction command (State.get ())
                Screen.showBoard ()
            | UseObject ->
                State.get ()
                |> performUseObjectAction command
                |> Turn.next
                Screen.showBoard ()
            | UseItem ->
                State.get ()
                |> useItem
                |> Turn.next
                Screen.showBoard ()
            | Harvest -> 
                State.get () 
                |> Actions.performHarvest
                |> Turn.next
                Screen.showBoard ()
            | ShowItems ->
                showItems ()
                //Screen.showBoard ()
            | ShowEquipment ->
                showEquipment ()
                Screen.showBoard ()
            | ShowMessages ->
                Screen.showMessages ()
            | Wear ->
                State.get ()
                |> wear
                |> Turn.next
                Screen.showBoard ()
            | Eat ->
                State.get ()
                |> eat
                |> Turn.next
                Screen.showBoard ()
            | TakeOff ->
                State.get ()
                |> takeOff
                |> Turn.next
                Screen.showBoard ()
            | PourLiquid ->
                State.get ()
                |> pourLiquid
                |> Turn.next
                Screen.showBoard ()
            | Help ->
                showHelpKeyCommands ()
                Screen.showBoard ()

    let initialize () =
        let thePlayer = Predefined.Classes.buildCharacterByPlayerClass "Soldier"

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
            //ignore (result.Add("Knife"))
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
                            TemporaryModifiers = [];
                            AvailableReplicationRecipes = getInitialReplicationRecipes;
                            MainMapDetails = Array2D.init boardWidth boardHeight (fun x y -> if mainMapPoint.X = x && mainMapPoint.Y = y then { PointOfInterest = Some("Your ship's crash site")} else { PointOfInterest = Option.None});
                            Settings = { HighlightPointsOfInterest = false }
                        }

        State.set entryState

    let mainLoop () =
        let rec loop printAll =
            let consoleKeyInfo = if printAll then new ConsoleKeyInfo('5', ConsoleKey.NumPad5, false, false, false) else Screen.readKey()
            try
                makeAction consoleKeyInfo
                loop false
            with QuitException -> ()
        loop true      

    let clearCharacterStates state = 
        let characters = 
            Board.getFilteredPlaces (fun item -> item.Character.IsSome) state.Board
            |> List.map (fun (_, item) -> item.Character.Value)
        characters |> List.iter (fun item -> 
            item.ResetVolatileStates()
            item.TickBiologicalClock()
            )
        state

    let evaluateTemporaryModifiers (state : State) =
        let stillActiveModifiers = state.TemporaryModifiers |> List.filter (fun x -> x.TurnOffOnTurnNr > state.TurnNumber)
        let rec evaluateAllModifiers (state : State) =
            match state.TemporaryModifiers with
            | [] -> state
            | head :: tail ->
                if head.TurnOnOnTurnNr <= state.TurnNumber && head.TurnOffOnTurnNr >= state.TurnNumber then
                    let currentEffectTurn = state.TurnNumber - head.TurnOnOnTurnNr
                    let relativeLastEffectTurn = head.TurnOffOnTurnNr - head.TurnOnOnTurnNr
                    evaluateAllModifiers {( state |> head.StateChangeFunction currentEffectTurn relativeLastEffectTurn  ) with TemporaryModifiers = tail}
                else
                    evaluateAllModifiers { state with TemporaryModifiers = tail}
        let modifiedState = state |> evaluateAllModifiers
        {modifiedState with TemporaryModifiers = stillActiveModifiers}
    
    let subscribeHandlers () =
        Turn.subscribe handleMonsters
        Turn.subscribe clearCharacterStates
        Turn.subscribe setVisibilityStates
        Turn.subscribe evaluateTemporaryModifiers  
        Turn.subscribe (
            fun state -> 
            {state with TurnNumber = state.TurnNumber + 1}
         )

    [<EntryPoint>]
    let main args =    
        subscribeHandlers ()
        initialize ()
        mainLoop ()    
        0