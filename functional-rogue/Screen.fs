﻿module Screen

open System
open System.Drawing
open Board
open State
open Sight
open Items
open Player
open Characters

type ScreenAgentMessage =
    | ShowBoard of State
    | ShowMainMenu of AsyncReplyChannel<MainMenuReply>
    | ShowChooseItemDialog of ShowChooseItemDialogRequest
    | ShowEquipmentDialog of ChooseEquipmentDialogRequest
    | ShowMessages of State
    | ShowOptions of seq<char * string>
    | SetCursorPositionOnBoard of Point * State
and MainMenuReply = {
    Name: String
} 
and ChooseItemDialogReply = {
    Selected: list<Item>
} 
and ChooseItemDialogRequest = {
    Items: list<Item>
    CanSelect: bool
}
and ChooseEquipmentDialogRequest = {
    Items: list<Item>
    CanSelect: bool
} 
and ShowChooseItemDialogRequest = {
    State: State;
    Filter: Item -> bool
}


type textel = {
    Char: char;
    BGColor: ConsoleColor;
    FGColor: ConsoleColor
} 
    
let private empty = {Char = ' '; FGColor = ConsoleColor.Gray; BGColor = ConsoleColor.Black}

type private screen = textel[,]

let boardFrameSize = new Size(60, 23)
let private screenSize = new Size(79, 24)
let private leftPanelPos = new Rectangle(61, 0, 19, 24)
let private letterByInt (int: int) = Convert.ToChar(Convert.ToInt32('a') + int - 1)

let private screenWritter () =    
    let writeBoard (board: Board) (boardFramePosition: Point) sightRadius (screen: screen) = 
        let toTextel item =  
            if item.WasSeen then
                let result = 
                    let character = 
                        if item.IsSeen && item.Character.IsSome then
                            match item.Character.Value.Type with
                            | Avatar -> {Char = '@'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                            | Monster -> {Char = item.Character.Value.Appearance; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Red}
                            | NPC -> {Char = 'P'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.White}
                         else
                            empty
                    if character <> empty then
                        character
                    else
                        match item.Items with
                        | h::_ when not <| Set.contains item.Tile obstacles -> 
                                match h.Type with 
                                | Stick -> {Char = '|'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                                | Rock  -> {Char = '*'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                                | Sword -> {Char = '/'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                                | Hat -> {Char = ']'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                                | Corpse -> {Char = '%'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                                | Tool -> {Char = '['; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                        | _ -> 
                            match item.Ore with
                            | Iron(_) -> {Char = '$'; FGColor = ConsoleColor.Black; BGColor = ConsoleColor.Gray}
                            | Gold(_) -> {Char = '$'; FGColor = ConsoleColor.Black; BGColor = ConsoleColor.Yellow}
                            | Uranium(_) -> {Char = '$'; FGColor = ConsoleColor.Black; BGColor = ConsoleColor.Green}
                            | CleanWater(_) -> {Char = '~'; FGColor = ConsoleColor.Blue; BGColor = ConsoleColor.Black}
                            | ContaminatedWater(_) -> {Char = '~'; FGColor = ConsoleColor.Blue; BGColor = ConsoleColor.Black}
                            | _ ->
                                match item.Tile with
                                | Wall ->  {Char = '#'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                                | Floor -> {Char = '.'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                                | OpenDoor -> {Char = '/'; FGColor = ConsoleColor.DarkGray; BGColor = ConsoleColor.Black}
                                | ClosedDoor -> {Char = '+'; FGColor = ConsoleColor.DarkGray; BGColor = ConsoleColor.Black}
                                | Grass -> {Char = '.'; FGColor = ConsoleColor.DarkGreen; BGColor = ConsoleColor.Black}
                                | Tree -> {Char = 'T'; FGColor = ConsoleColor.DarkGreen; BGColor = ConsoleColor.Black}
                                | SmallPlants -> {Char = '*'; FGColor = ConsoleColor.DarkGreen; BGColor = ConsoleColor.Black}
                                | Bush -> {Char = '&'; FGColor = ConsoleColor.DarkGreen; BGColor = ConsoleColor.Black}
                                | Glass -> {Char = '#'; FGColor = ConsoleColor.Blue; BGColor = ConsoleColor.Black}
                                | Sand -> {Char = '.'; FGColor = ConsoleColor.Yellow; BGColor = ConsoleColor.Black}
                                | Water -> {Char = '~'; FGColor = ConsoleColor.Blue; BGColor = ConsoleColor.Black}
                                | StairsDown -> {Char = '>'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                                | StairsUp -> {Char = '<'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                                | MainMapForest -> {Char = '&'; FGColor = ConsoleColor.DarkGreen; BGColor = ConsoleColor.Black}
                                | MainMapGrassland -> {Char = '"'; FGColor = ConsoleColor.Green; BGColor = ConsoleColor.Black}
                                | MainMapMountains -> {Char = '^'; FGColor = ConsoleColor.Gray; BGColor = ConsoleColor.Black}
                                | MainMapWater -> {Char = '~'; FGColor = ConsoleColor.Blue; BGColor = ConsoleColor.Black}
                                | MainMapCoast -> {Char = '.'; FGColor = ConsoleColor.Yellow; BGColor = ConsoleColor.Black}
                                | _ -> empty
                if not item.IsSeen then {result with FGColor = ConsoleColor.DarkGray } else result
            else empty
        
        for x in 0..boardFrameSize.Width - 1 do
            for y in 0..boardFrameSize.Height - 1 do
                // move board                                
                let virtualX = x + boardFramePosition.X
                let virtualY = y + boardFramePosition.Y
                screen.[x, y] <- toTextel board.Places.[virtualX, virtualY]
        screen      
        
    let writeString (position: Point) (text: String) (screen: screen) = 
        let x = position.X
        let y = position.Y
        let length = min (screenSize.Width - x) text.Length
        text.Substring(0, length)
        |> String.iteri (fun i char -> 
            screen.[x + i, y] <- {empty with Char = char})
        screen
    
    let cleanPartOfScreen (point: Point) (size: Size) (screen: screen) =
        for x in (point.X)..(point.X + size.Width - 1)do
            for y in (point.Y)..(point.Y + size.Height - 1) do
                screen.[x, y] <- empty
        screen

    let cleanScreen (screen: screen) =
        cleanPartOfScreen (Point(0,0)) (Size(screenSize.Width, screenSize.Height)) screen

    let writeStats state screen =
        screen 
        |> writeString leftPanelPos.Location (sprintf "HP: %d/%d" state.Player.CurrentHP state.Player.MaxHP)
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 1)) (sprintf "%s the rogue" state.Player.Name)
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 2)) (sprintf "Ma: %d/%d" state.Player.CurrentHP state.Player.MaxHP)
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 3)) (sprintf "Iron: %s" <| state.Player.Iron.ToString())
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 4)) (sprintf "Gold: %s" <| state.Player.Gold.ToString())
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 5)) (sprintf "Uranium: %s" <| state.Player.Uranium.ToString())
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 6)) (sprintf "Water: %s" <| state.Player.Water.ToString())
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 7)) (sprintf "Cont. Water: %s" <| state.Player.ContaminatedWater.ToString())
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 8)) (sprintf "Turn: %d" <| state.TurnNumber)
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 9)) (sprintf "Map Level: %d" <| state.Board.Level)

    let writeLastTurnMessageIfAvailable state screen =
        if( state.UserMessages.Length > 0 && (fst (state.UserMessages.Head)) = state.TurnNumber - 1) then
            screen
            |> cleanPartOfScreen (Point(0,(screenSize.Height - 1))) (Size(screenSize.Width, 1))
            |> writeString (point 0 (screenSize.Height - 1)) (snd state.UserMessages.Head)
        else
            screen |> cleanPartOfScreen (Point(0,(screenSize.Height - 1))) (Size(screenSize.Width, 1))

    let writeTemporalMessage (message: string) screen =
        screen
        |> cleanPartOfScreen (Point(0,(screenSize.Height - 1))) (Size(screenSize.Width, 1))
        |> writeString (point 0 (screenSize.Height - 1)) (message)

    let writeAllMessages (state : State) screen =
        let rec writeMessagesRecursively (messages: (int*string) list) screen lineNumber =
            match messages with
            | head :: tail -> writeMessagesRecursively tail (screen |> (writeString (Point(0, lineNumber)) (sprintf "Turn: %d - %s" (fst head) (snd head)))) (lineNumber + 1)
            | [] -> screen
        writeMessagesRecursively (state.UserMessages |> Seq.take (Math.Min(state.UserMessages.Length, screenSize.Height - 1)) |> Seq.toList) screen 0
            
    let listAllItems (items : Item list) (shortCuts : Map<char, Item>) screen = 
        //let plainItems = items |> Seq.choose (function | Gold(_) -> Option.None | Plain(_, itemProperties) -> Some itemProperties)
        
        let writeProperties = seq {
            for i, item in Seq.mapi (fun i item -> i, item) items do
                let pos = point 1 i                
                let char = match findShortCut shortCuts item with Some(value) -> value.ToString() | _ -> ""                
                yield writeString pos (sprintf "%s (id=%s): %s" char (item.Id.ToString()) (itemShortDescription item))
        }
        screen |>> writeProperties

    let listWornItems screen =
        let writeIfExisits (item : option<Item>) = 
            if item.IsSome then itemShortDescription (item.Value) else ""

        let writeProperties (allItems : Item list) (wornItems : WornItems) = [
            (writeString (new Point(1,0)) (sprintf "Head - %s"  (writeIfExisits wornItems.Head )));
            (writeString (new Point(1,1)) (sprintf "In Hand - %s" (writeIfExisits wornItems.Hand )));
            (writeString (new Point(1,2)) (sprintf "On Torso - %s" (writeIfExisits wornItems.Torso )));
            (writeString (new Point(1,3)) (sprintf "On Legs - %s" (writeIfExisits wornItems.Legs )));
        ]

        let currentState = State.get ()
        screen |>> (writeProperties currentState.Player.Items currentState.Player.WornItems)

    let showOptions (options : seq<char * string>) screen =
        let write = options |> Seq.mapi (fun i (char, message) -> 
            writeString (new Point(1, i)) (sprintf "%s: %s" (char.ToString()) message);
        ) 
        screen |>> write                
               
    let refreshScreen (oldScreen: screen) (newScreen: screen) = 
        let changes = seq {
            for x in 0..screenSize.Width - 1 do
                for y in 0..screenSize.Height - 1 do
                    if oldScreen.[x, y] <> newScreen.[x, y] then yield (point x y, newScreen.[x, y])
        }
        changes
        |> Seq.iter (fun (position, textel) -> 
            Console.SetCursorPosition(position.X, position.Y)
            Console.BackgroundColor <- textel.BGColor
            Console.ForegroundColor <- textel.FGColor
            Console.Write(textel.Char)
        )
        Console.SetCursorPosition(screenSize.Width, screenSize.Height)

    MailboxProcessor<ScreenAgentMessage>.Start(fun inbox ->
        let rec loop screen = async {
            let! msg = inbox.Receive()

            match msg with
            | ShowBoard(state) -> 
                let newScreen = 
                    screen
                    |> Array2D.copy
                    |> writeBoard state.Board state.BoardFramePosition state.Player.SightRadius
                    |> writeStats state
                    |> writeLastTurnMessageIfAvailable state
                refreshScreen screen newScreen
                return! loop newScreen
            | ShowMessages(state) ->
                let newScreen =
                    screen
                    |> Array2D.copy
                    |> cleanScreen
                    |> writeAllMessages state
                refreshScreen screen newScreen
                return! loop newScreen                        
            | ShowMainMenu(reply) -> 
                let newScreen = 
                    screen
                    |> Array2D.copy
                    |> cleanScreen
                    |> writeString (point 1 1) "What is your name?"
                refreshScreen screen newScreen
                Console.SetCursorPosition(1, 2)
                let name = Console.ReadLine()
                reply.Reply({Name = name})
                return! loop newScreen  
            | ShowChooseItemDialog(request) ->                
                let itemsToShow =
                    List.filter request.Filter request.State.Player.Items                
                let newScreen =
                    screen
                    |> Array2D.copy
                    |> cleanScreen
                    |> if itemsToShow.Length > 0 then 
                           listAllItems itemsToShow request.State.Player.ShortCuts 
                       else 
                           writeString (point 1 1) "No items"
                refreshScreen screen newScreen
                return! loop newScreen
            | ShowEquipmentDialog(request) ->                
                let newScreen =
                    screen
                    |> Array2D.copy
                    |> cleanScreen
                    |> listWornItems
                refreshScreen screen newScreen
                return! loop newScreen
            | ShowOptions(request) ->
                let newScreen =
                    screen
                    |> Array2D.copy
                    |> cleanScreen
                    |> showOptions request
                refreshScreen screen newScreen
                return! loop newScreen
            | SetCursorPositionOnBoard(point, state) ->
                let realPosition = (Math.Min(boardFrameSize.Width, Math.Max(0, point.X - state.BoardFramePosition.X)),
                                    Math.Min(boardFrameSize.Height, Math.Max(0, point.Y - state.BoardFramePosition.Y)))
                let place = state.Board.Places.[point.X,point.Y]
                let description = Place.GetDescription place
                let newScreen =
                    screen
                    |> Array2D.copy
                    |> writeTemporalMessage description
                refreshScreen screen newScreen
                Console.SetCursorPosition(realPosition)
                return! loop newScreen

        }
        loop <| Array2D.create screenSize.Width screenSize.Height empty
    )

let private evaluateBoardFramePosition state = 
    let playerPosition = getPlayerPosition state.Board
    let frameView = new Rectangle(state.BoardFramePosition, boardFrameSize)
    let preResult =
        let x = inBoundary (playerPosition.X - (boardFrameSize.Width / 2)) 0 (boardWidth - boardFrameSize.Width) 
        let y = inBoundary (playerPosition.Y - (boardFrameSize.Height / 2)) 0 (boardHeight - boardFrameSize.Height)
        point x y                
    { state with BoardFramePosition = preResult }



let private agent = screenWritter ()
let showBoard () = agent.Post (ShowBoard(State.get ()))
let showMainMenu () = agent.PostAndReply(fun reply -> ShowMainMenu(reply))
let showChooseItemDialog items = agent.Post(ShowChooseItemDialog(items))
let showEquipmentItemDialog items = agent.Post(ShowEquipmentDialog(items))
let setCursorPositionOnBoard point state = agent.Post(SetCursorPositionOnBoard(point, state))
let showMessages () = agent.Post (ShowMessages(State.get ()))
let showOptions options  = agent.Post(ShowOptions(options))

Turn.subscribe evaluateBoardFramePosition