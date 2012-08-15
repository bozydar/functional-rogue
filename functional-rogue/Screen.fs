namespace FunctionalRogue

module Screen =

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
    
    let empty = {Char = ' '; FGColor = ConsoleColor.Gray; BGColor = ConsoleColor.Black}

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

    let itemToTextel (item : Item) =
        match item.Type with
        | Stick -> {Char = '|'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
        | Rock  -> {Char = '*'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
        | Sword | Knife -> {Char = '/'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
        | Hat -> {Char = ']'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
        | Corpse -> {Char = '%'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
        | OreExtractor(_) -> {Char = '['; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
        | Drone -> {Char = '^'; FGColor = ConsoleColor.Cyan; BGColor = ConsoleColor.Black}
        | Injector -> {Char = '!'; FGColor = ConsoleColor.Red; BGColor = ConsoleColor.Black}
        | SimpleContainer -> {Char = 'u'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}

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
                            itemToTextel h
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
                            | Tile.Water -> {Char = '~'; FGColor = ConsoleColor.Blue; BGColor = ConsoleColor.Black}
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
            if not item.IsSeen then {result with FGColor = ConsoleColor.DarkGray }
            else
                if item.Features |> List.exists (fun feature -> match feature with | OnFire(value) -> true | _ -> false) then
                    let randomResult = rnd 10
                    match randomResult with
                    | value when value < 1 -> {Char = '&'; FGColor = ConsoleColor.Red; BGColor = ConsoleColor.Black}
                    | value when value < 2 -> {Char = '&'; FGColor = ConsoleColor.Yellow; BGColor = ConsoleColor.Black}
                    | _ -> result
                else
                    result
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
        | ReadKey of AsyncReplyChannel<ReadKeyReply>
        | ShowBoard of State
        | ShowMainMenu of AsyncReplyChannel<MainMenuReply>
        | ShowChooseItemDialog of ShowChooseItemDialogRequest
        | ShowEquipmentDialog of ChooseEquipmentDialogRequest
        | ShowMessages of State
        | ShowOptions of seq<char * string>
        | SetCursorPositionOnBoard of Point * State
        | DisplayComputerScreen of ScreenContentBuilder * State
        | ShowFinishScreen of State
        | ShowDialog of Dialog.Dialog * Dialog.Result * Rectangle * AsyncReplyChannel<ShowDialogReply>
        | ShowBoardAnimationFromFrame of State * int * (int -> textel[,] -> textel[,] option)

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
    and ShowDialogReply = { TopLinesClipped : int; BottomLinesClipped : int}
    and ReadKeyReply = { ConsoleKeyInfo : ConsoleKeyInfo }


    let getHighlightForTile (board : Board) x y =
        if board.IsMainMap then
            let tileDetails = (State.get()).MainMapDetails.[x,y]
            let settings = (State.get()).Settings
            if settings.HighlightPointsOfInterest && tileDetails.PointOfInterest.IsSome then
                Some(ConsoleColor.Yellow)
            else
                Option.None
        else
            Option.None

    let evaluateBoardFramePosition state = 
        let playerPosition = getPlayerPosition state.Board
        let frameView = new Rectangle(state.BoardFramePosition, boardFrameSize)
        let preResult =
            let x = inBoundary (playerPosition.X - (boardFrameSize.Width / 2)) 0 (boardWidth - boardFrameSize.Width) 
            let y = inBoundary (playerPosition.Y - (boardFrameSize.Height / 2)) 0 (boardHeight - boardFrameSize.Height)
            point x y                
        { state with BoardFramePosition = preResult }

    type Facade () as this = 
        [<DefaultValue>] val mutable Agent : MailboxProcessor<ScreenAgentMessage> 

        member this.Post = this.Agent.Post 
        member this.PostAndReply (buildMessage) = this.Agent.PostAndReply (buildMessage)

    let agent = Facade ()

    let showBoard () = agent.Post (ShowBoard(State.get ()))
    let showAnimation animationFunction = agent.Post (ShowBoardAnimationFromFrame(State.get (), 0, animationFunction))
    //let showMainMenu () = agent.PostAndReply(fun reply -> ShowMainMenu(reply))
    let showChooseItemDialog items = agent.Post(ShowChooseItemDialog(items))
    let showEquipmentItemDialog items = agent.Post(ShowEquipmentDialog(items))
    let setCursorPositionOnBoard point state = agent.Post(SetCursorPositionOnBoard(point, state))
    let displayComputerScreen content = agent.Post(DisplayComputerScreen(content, State.get()))
    let showMessages () = agent.Post (ShowMessages(State.get ()))
    let showOptions options  = agent.Post(ShowOptions(options))
    let showFinishScreen state  = agent.Post(ShowFinishScreen(state))
    let readKey () = (agent.PostAndReply (ReadKey)).ConsoleKeyInfo
    let rec showDialog (dialog : Dialog.Dialog, dialogResult : Dialog.Result) : Dialog.Result = 
        let startingPosition = new Rectangle(0, 0, screenSize.Width, screenSize.Height)
        let findMenuItemsInDialog key dialog : option<Dialog.Widget> = 
            dialog    
            |> Seq.tryPick (function 
                | Dialog.Action(itemKey, _, _, _) as item when Keyboard.isKeyInput itemKey key -> Some(item)
                | Dialog.Subdialog(itemKey, _, innerDialog) as item when Keyboard.isKeyInput itemKey key -> Some(item)
                | Dialog.Option(itemKey, _, varName, _) as item when Keyboard.isKeyInput itemKey key -> Some(item)
                | _ -> None)    
        let rec loop dialogResult position : Dialog.Result =
            let showDialogReply = agent.PostAndReply (fun reply -> ShowDialog(dialog, dialogResult, position, reply))
            let key = Console.ReadKey(true)
            let selectedWidget = 
                (key, dialog)
                ||> findMenuItemsInDialog    
            if selectedWidget.IsSome then
                match selectedWidget.Value with
                    | Dialog.Action(_, _, varName, value) -> 
                        Dialog.replaceWith (dialogResult, Dialog.newResult [(varName, value)])
                    // TODO: Check this!!! I think that should be only one call to agent in whole showDialog fun
                    | Dialog.Subdialog(_, _, subdialog) -> 
                        let subResult = showDialog (subdialog, dialogResult)
                        Dialog.replaceWith (subResult, loop dialogResult position)
                    | Dialog.Option(_, _, varName, optionItems) ->        
                        let foundValueIndex =
                            if dialogResult.ContainsKey(varName) then
                                let value = dialogResult.Item(varName)
                                optionItems
                                |> List.tryFindIndex (function 
                                    | _, selectedValue when selectedValue = value -> true
                                    | _ -> false)
                             else
                                None
                        let index = if foundValueIndex.IsSome then foundValueIndex.Value else -1
                        let newIndex = (index + 1) % (List.length optionItems)
                        let _, newValue = optionItems.[newIndex]
                        let result = Dialog.replaceWith (dialogResult, Dialog.newResult ([(varName, newValue)]))
                    
                        loop result position
                    | _ -> loop dialogResult position
            else
                let newPosition = 
                    match key with // moving in X axi is not possible right now
                    | Key ConsoleKey.UpArrow when showDialogReply.TopLinesClipped > 0 -> new Rectangle(position.X, position.Y - 1, position.Width, position.Height)
                    | Key ConsoleKey.DownArrow when showDialogReply.BottomLinesClipped > 0 -> new Rectangle(position.X, position.Y + 1, position.Width, position.Height)
                    | Key ConsoleKey.PageUp when showDialogReply.TopLinesClipped > 0 -> new Rectangle(position.X, position.Y - screenSize.Height, position.Width, position.Height)
                    | Key ConsoleKey.PageDown when showDialogReply.BottomLinesClipped > 0 -> new Rectangle(position.X, position.Y + screenSize.Height, position.Width, position.Height)
                    | _ -> position
                loop dialogResult newPosition
        loop dialogResult startingPosition
    

    let chooseListItemThroughPagedDialog (title : string) (mapToName : 'T -> string ) (listItems : 'T list) =
        let rec chooseFromPage (pageNr : int) (title : string) (mapToName : 'T -> string ) (listItems : 'T list) =
            let letters = ['a'..'z'] |> List.map( fun item -> Input.Char(item) )
            let itemsPerPage = 10

            let dialog = new Dialog.Dialog(seq {
                yield Dialog.Title(title)
                yield! listItems 
                    |> Seq.skip (pageNr * itemsPerPage) 
                    |> Seq.truncate itemsPerPage 
                    |> Seq.mapi (fun i item -> Dialog.Action(letters.[i], mapToName item, "result", i.ToString()))
                if pageNr > 0 then yield Dialog.Action(Input.Char '-', "[prev]", "result", "-")
                if listItems.Length > (pageNr * itemsPerPage + itemsPerPage) then yield Dialog.Action(Input.Char '+', "[next]", "result", "+")
                yield Dialog.Action(Input.Char '*', "[escape]", "result", "*")
            })
            let dialogResult = showDialog(dialog, Dialog.emptyResult)
            match dialogResult.Item("result") with
            | "*" -> Option.None
            | "+" -> listItems |> chooseFromPage (pageNr + 1) title mapToName 
            | "-" -> listItems |> chooseFromPage (pageNr - 1) title mapToName 
            | _ -> 
                let chosenItem = listItems.[Int32.Parse(dialogResult.Item("result")) + (pageNr * itemsPerPage)]
                Some(chosenItem)
        listItems |> chooseFromPage 0 title mapToName
    
    //    else 
    //        // Textbox
    //        let matcher1 = function 
    //            | Dialog.Textbox(variableName) -> Some(variableName) 
    //            | _ -> None
    //        let matcher2 arg = matcher1 arg <> None
    //        let variableName = dialog |> List.tryPick matcher1
    //        if variableName.IsSome then
    //            let positionY = dialog |> List.findIndex matcher2
    //            let rec loop () = 
    //                Console.SetCursorPosition(0, positionY)
    //                let value = Console.ReadLine()
    //                if String.IsNullOrWhiteSpace(value) then 
    //                    loop ()
    //                else 
    //                    Dialog.newResult [(variableName.Value, [value])]
    //            loop ()
    //        else dialogResult