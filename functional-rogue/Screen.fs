﻿module Screen

open System
open System.Drawing
open Board
open State
open Sight
open Player
open Characters

type textel = {
    Char: char;
    BGColor: ConsoleColor;
    FGColor: ConsoleColor
} 
    
let private empty = {Char = ' '; FGColor = ConsoleColor.Gray; BGColor = ConsoleColor.Black}

type private screen = textel[,]

type ColorTheme =
    | ComputerTheme

let breakStringIfTooLong (maxWidth: int) (text: string) =
        let joinWordsIfNotTooLong (words: string list) =
            let result = words |> List.fold (fun (acc: string list) word -> if (acc.Head.Length + word.Length + 1) > maxWidth then [word] @ acc else [acc.Head + (if acc.Head.Length > 0 then " " else "") + word] @ acc.Tail) [""]
            result |> List.rev
        if text.Length > maxWidth then
            let words = text.Split([|" "|], StringSplitOptions.None)
            joinWordsIfNotTooLong (List.ofArray words)
        else
            [text]

let toTextel item (highlighOption : ConsoleColor option) =  
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
                        | Sword | Knife -> {Char = '/'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
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
                        let mainMapBackground = if highlighOption.IsSome then highlighOption.Value else ConsoleColor.Black
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
                        | Computer -> {Char = '#'; FGColor = ConsoleColor.Red; BGColor = ConsoleColor.Black}
                        | Replicator -> {Char = '_'; FGColor = ConsoleColor.Red; BGColor = ConsoleColor.Black}
                        | MainMapForest -> {Char = '&'; FGColor = ConsoleColor.DarkGreen; BGColor = mainMapBackground}
                        | MainMapGrassland -> {Char = '"'; FGColor = ConsoleColor.Green; BGColor = mainMapBackground}
                        | MainMapMountains -> {Char = '^'; FGColor = ConsoleColor.Gray; BGColor = mainMapBackground}
                        | MainMapWater -> {Char = '~'; FGColor = ConsoleColor.Blue; BGColor = mainMapBackground}
                        | MainMapCoast -> {Char = '.'; FGColor = ConsoleColor.Yellow; BGColor = mainMapBackground}
                        | _ -> empty
        if not item.IsSeen then {result with FGColor = ConsoleColor.DarkGray } else result
    else empty
        

type ScreenContentBuilder (maxWidth: int, theme: ColorTheme) =
    let mutable textelArraysList = List.empty<textel[]>

    let textfg =
        match theme with
        | ComputerTheme -> ConsoleColor.DarkGreen

    let textbg =
        match theme with
        | ComputerTheme -> ConsoleColor.Black

    let selectablefg =
        match theme with
        | ComputerTheme -> ConsoleColor.DarkGreen

    let selectablebg =
        match theme with
        | ComputerTheme -> ConsoleColor.Yellow

    let separatorTextel =
        match theme with
        | ComputerTheme -> {Char = '='; FGColor = textfg; BGColor = textbg}

    let padding =
        match theme with
        | ComputerTheme -> 1

    let oneLineOptionsSeparator = Array.create 2 {Char = ' '; FGColor = textfg; BGColor = textbg}

    do if padding > 0 then
        textelArraysList <- textelArraysList @ List.init padding (fun i -> [|{Char = ' '; FGColor = textfg; BGColor = textbg}|])

    let leftPadding =
        Array.create padding {Char = ' '; FGColor = textfg; BGColor = textbg}

    let stringToTextelArray (text: string) (fgColor: ConsoleColor) (bgColor: ConsoleColor) =
            text.ToCharArray() |> Array.map (fun item -> {Char = item; FGColor = fgColor; BGColor = bgColor})

    member this.AddString (text: string) =
        let brokenText = breakStringIfTooLong (maxWidth - 2 * padding) text
        textelArraysList <- brokenText |> List.fold (fun (acc: textel[] list) line -> acc @ [Array.append leftPadding (stringToTextelArray line textfg textbg)]) textelArraysList

    member this.AddPlacesArray (places: Place[,]) =
        let width = places |> Array2D.length1
        let widthWithPadding = width + padding
        let reverseTextelsList =
            places
            |> Seq.cast<Place>
            |> Seq.toList
            |> List.permute (fun i -> (i % width) * width + (i / width))    //changes 'by columns' to 'by rows'
            |> Seq.ofList
            |> Seq.fold (fun (acc: textel[] list) place -> if not(acc.IsEmpty) && acc.Head.Length < widthWithPadding then [Array.append acc.Head [|toTextel place Option.None|]] @ acc.Tail else [Array.append leftPadding [|toTextel place Option.None|]] @ acc) List.empty<textel[]> 
        textelArraysList <- textelArraysList @ List.rev reverseTextelsList
        ()

    member this.AddSelectables (putInOneLine: bool) (selectables: (string * string) list) =
        let asTextels = selectables |> List.map (fun item -> Array.append (stringToTextelArray (fst item) selectablefg selectablebg) (stringToTextelArray (snd item) textfg textbg))
        if putInOneLine then
            let inOneLine = asTextels |> List.fold (fun acc item -> if (acc |> Array.length = 0) then item else  [acc;oneLineOptionsSeparator;item] |> Array.concat) Array.empty<textel>
            textelArraysList <- textelArraysList @ [Array.append leftPadding inOneLine]
        else
            textelArraysList <- textelArraysList @ (asTextels |> List.map (fun item -> Array.append leftPadding item))
        ()

    member this.AddEmptyLine () =
        textelArraysList <- textelArraysList @ [[||]]

    member this.AddEmptyLineIfPreviousNotEmpty () =
        if textelArraysList.Length > 0 && textelArraysList.[textelArraysList.Length - 1].Length > 0 then
            textelArraysList <- textelArraysList @ [[||]]

    member this.AddSeparator () =
        if textelArraysList.Length > 0 then
            let length = textelArraysList.[textelArraysList.Length - 1].Length
            textelArraysList <- textelArraysList @ [Array.append leftPadding (Array.create (length-padding) separatorTextel)]
            ()

    member this.ToTextels () =
        textelArraysList


let boardFrameSize = new Size(60, 23)
let private screenSize = new Size(79, 24)
let private leftPanelPos = new Rectangle(61, 0, 19, 24)
let private letterByInt (int: int) = Convert.ToChar(Convert.ToInt32('a') + int - 1)


type ScreenAgentMessage =
    | ShowBoard of State
    | ShowMainMenu of AsyncReplyChannel<MainMenuReply>
    | ShowChooseItemDialog of ShowChooseItemDialogRequest
    | ShowEquipmentDialog of ChooseEquipmentDialogRequest
    | ShowMessages of State
    | ShowOptions of seq<char * string>
    | SetCursorPositionOnBoard of Point * State
    | DisplayComputerScreen of ScreenContentBuilder * State
    | ShowFinishScreen of State
    | ShowDialog of Dialog.Dialog * AsyncReplyChannel<unit>

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

let getHighlightForTile (board : Board) x y =
    if board.Type = LevelType.MainMap then
        let tileDetails = (State.get()).MainMapDetails.[x,y]
        let settings = (State.get()).Settings
        if settings.HighlightPointsOfInterest && tileDetails.PointOfInterest.IsSome then
            Some(ConsoleColor.Yellow)
        else
            Option.None
    else
        Option.None

let private screenWritter () =    
    let writeBoard (board: Board) (boardFramePosition: Point) sightRadius (screen: screen) = 
        
        for x in 0..boardFrameSize.Width - 1 do
            for y in 0..boardFrameSize.Height - 1 do
                // move board                                
                let virtualX = x + boardFramePosition.X
                let virtualY = y + boardFramePosition.Y
                screen.[x, y] <- toTextel board.Places.[virtualX, virtualY] (getHighlightForTile board virtualX virtualY)
        screen      
        
    let writeString (position: Point) (text: String) (screen: screen) = 
        let x = position.X
        let y = position.Y
        let length = min (screenSize.Width - x) text.Length
        text.Substring(0, length)
        |> String.iteri (fun i char -> 
            screen.[x + i, y] <- {empty with Char = char})
        screen

    let writeDecoratedText (position: Point) (decoratedText: Dialog.DecoratedText) (screen: screen) = 
        let x = position.X
        let y = position.Y
        let text = decoratedText.Text
        let length = min (screenSize.Width - x) text.Length
        text.Substring(0, length)
        |> String.iteri (fun i char -> 
            screen.[x + i, y] <- {BGColor = decoratedText.BGColor; FGColor = decoratedText.FGColor; Char = char})
        screen

    let writeStrings (position: Point) (lines: String list) (fgcolor: ConsoleColor) (screen: screen) =
        let brokenLines = lines |> List.fold (fun (acc: string list) line -> acc @ (line |> breakStringIfTooLong boardFrameSize.Width)) []
        let x = position.X
        let y = position.Y
        let rec writeAllLines x y (lines: String list) (screen: screen) =
            match lines with
            | head :: tail ->
                let length = min (screenSize.Width - x) head.Length
                head.Substring(0, length)
                |> String.iteri (fun i char -> 
                    screen.[x + i, y] <- {empty with Char = char; FGColor = fgcolor})
                writeAllLines x (y+1) tail screen
            |[] -> screen
        writeAllLines x y brokenLines screen
    
    let writeFromScreenContentBuilder (position: Point) (builder: ScreenContentBuilder) (fgcolor: ConsoleColor) (screen: screen) =
        let x = position.X
        let y = position.Y
        let rec writeAllLines x y (textels: textel[] list) (screen: screen) =
            match textels with
            | head :: tail ->
                head
                |> Array.iteri (fun i item -> screen.[x + i, y] <- item)
                writeAllLines x (y+1) tail screen
            |[] -> screen
        writeAllLines x y (builder.ToTextels()) screen

    let cleanPartOfScreen (point: Point) (size: Size) (screen: screen) =
        for x in (point.X)..(point.X + size.Width - 1) do
            for y in (point.Y)..(point.Y + size.Height - 1) do
                screen.[x, y] <- empty
        screen

    let cleanScreen (screen: screen) =
        cleanPartOfScreen (Point(0,0)) (Size(screenSize.Width, screenSize.Height)) screen

    let writeStats state screen =
        screen 
        |> writeString leftPanelPos.Location (sprintf "HP: %d/%d" state.Player.CurrentHP state.Player.MaxHP)
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 1)) (sprintf "%s the rogue    " state.Player.Name)
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 2)) (sprintf "Ma: %d/%d       " state.Player.CurrentHP state.Player.MaxHP)
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 3)) (sprintf "Iron: %s        " <| state.Player.Iron.ToString())
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 4)) (sprintf "Gold: %s        " <| state.Player.Gold.ToString())
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 5)) (sprintf "Uranium: %s     " <| state.Player.Uranium.ToString())
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 6)) (sprintf "Water: %s       " <| state.Player.Water.ToString())
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 7)) (sprintf "Cont. Water: %s " <| state.Player.ContaminatedWater.ToString())
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 8)) (sprintf "Turn: %d        " <| state.TurnNumber)
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 9)) (sprintf "Map Level: %d   " <| state.Board.Level)

    let writeFinishScreen state screen =
        let position = point 1 0
        screen 
        |> writeString position "You have been died. Nobody heard your scream."
        |> writeString (position + (new Size(0, 1))) (sprintf "HP: %d/%d" state.Player.CurrentHP state.Player.MaxHP)
        |> writeString (position + (new Size(0, 2))) (sprintf "%s the rogue" state.Player.Name)
        |> writeString (position + (new Size(0, 3))) (sprintf "Ma: %d/%d" state.Player.CurrentHP state.Player.MaxHP)
        |> writeString (position + (new Size(0, 4))) (sprintf "Iron: %s" <| state.Player.Iron.ToString())
        |> writeString (position + (new Size(0, 5))) (sprintf "Gold: %s" <| state.Player.Gold.ToString())
        |> writeString (position + (new Size(0, 6))) (sprintf "Uranium: %s" <| state.Player.Uranium.ToString())
        |> writeString (position + (new Size(0, 7))) (sprintf "Water: %s" <| state.Player.Water.ToString())
        |> writeString (position + (new Size(0, 8))) (sprintf "Cont. Water: %s" <| state.Player.ContaminatedWater.ToString())
        |> writeString (position + (new Size(0, 9))) (sprintf "Turn: %d" <| state.TurnNumber)
        |> writeString (position + (new Size(0, 10))) (sprintf "Map Level: %d" <| state.Board.Level)

    let writeLastTurnMessageIfAvailable state screen =
        if( state.UserMessages.Length > 0 && (fst (state.UserMessages.Head)) = state.TurnNumber) then
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
            (writeString (new Point(1,0)) (sprintf "Head - %s     "  (writeIfExisits wornItems.Head )));
            (writeString (new Point(1,1)) (sprintf "In Hand - %s  " (writeIfExisits wornItems.Hand )));
            (writeString (new Point(1,2)) (sprintf "On Torso - %s " (writeIfExisits wornItems.Torso )));
            (writeString (new Point(1,3)) (sprintf "On Legs - %s  " (writeIfExisits wornItems.Legs )));
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

    let showDialog dialog screen =        
        let sequence = seq {            
            for i, item in dialog |> Seq.mapi (fun i item -> i, item) do
                match item with
                | Dialog.Title(text) -> 
                    yield writeDecoratedText (point 0 i) (Dialog.newDecoratedText text  ConsoleColor.White ConsoleColor.Black)
                | Dialog.Label(text) ->
                    yield writeDecoratedText (point 0 i) (Dialog.newDecoratedText text  ConsoleColor.Black ConsoleColor.Gray)
                | Dialog.Raw(decoratedText) ->
                    yield writeDecoratedText (point 0 i) decoratedText
                | Dialog.Menu(_, items) ->
                    for j, itemj in items |> List.mapi (fun i item -> i, item) do
                        match itemj with
                        | Dialog.Item(input, text, _) -> 
                            let dt1 = Dialog.newDecoratedText (input.ToString() + " - " + text) ConsoleColor.Black ConsoleColor.White 
                            let dt2 = Dialog.newDecoratedText text ConsoleColor.Black ConsoleColor.Gray
                            yield writeDecoratedText (point 0 (i + j + 1)) dt1
                            yield writeDecoratedText (point 4 (i + j + 1)) dt2
                    
                | _ -> yield self
        }           
        screen |>> sequence

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
                let description = Place.GetDescription place (if state.Board.Type = LevelType.MainMap then getMainMapTileDetails point.X point.Y else String.Empty)
                let newScreen =
                    screen
                    |> Array2D.copy
                    |> writeTemporalMessage description
                refreshScreen screen newScreen
                Console.SetCursorPosition(realPosition)
                return! loop newScreen
            | DisplayComputerScreen(content, state) ->
                let newScreen = 
                    screen
                    |> Array2D.copy
                    |> cleanPartOfScreen (Point(0,0)) boardFrameSize
                    |> writeFromScreenContentBuilder (Point(0,0)) content ConsoleColor.DarkGreen
                    |> writeStats state
                refreshScreen screen newScreen
                return! loop newScreen
            | ShowFinishScreen(state) ->
                let newScreen =
                    screen
                    |> Array2D.copy
                    |> cleanScreen
                    |> writeFinishScreen(state)                    
                refreshScreen screen newScreen
                return! loop newScreen
            | ShowDialog(dialog, reply) ->
                let newScreen =
                    screen
                    |> Array2D.copy
                    |> cleanScreen
                    |> showDialog(dialog)                    
                refreshScreen screen newScreen
                reply.Reply ()
                return! loop newScreen                
        }
        loop <| Array2D.create screenSize.Width screenSize.Height empty
    )

let evaluateBoardFramePosition state = 
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
let displayComputerScreen content = agent.Post(DisplayComputerScreen(content, State.get()))
let showMessages () = agent.Post (ShowMessages(State.get ()))
let showOptions options  = agent.Post(ShowOptions(options))
let showFinishScreen state  = agent.Post(ShowFinishScreen(state))
let showDialog dialog = 
    agent.PostAndReply (fun reply -> ShowDialog(dialog, reply))
    let findKeyInDialog key dialog = 
        dialog    
        |> List.tryPick (function 
            | Dialog.Menu(varName, items) -> 
                let preResult =
                    items
                    |> List.tryPick (function 
                        | Dialog.Item(itemKey, _, value) when itemKey = key -> Some(value)
                        | _ -> None)
                if preResult.IsSome then Some (varName, preResult.Value) else None
            | _ -> None)
    if List.exists (function | Dialog.Menu(_, _) -> true | _ -> false) dialog then        
        let rec loop () =
            let variableValuePair = 
                (Console.ReadKey(true).KeyChar, dialog)
                ||> findKeyInDialog    
            if variableValuePair.IsSome then 
                [variableValuePair.Value]
            else
                loop ()
        loop ()
    else
        List.empty