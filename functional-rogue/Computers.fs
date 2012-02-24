module Computers

open System
open State
open Screen
open Board
open Replication
open System.Drawing
open Predefined.Items

type ComputerNavigation =
    | MainMenu
    | Notes
    | Note
    | Doors
    | Door
    | Cameras
    | Camera
    | Replication
    | ReplicationItem

type ComputerCommand =
    | None
    | OpenDoor
    | CloseDoor
    | Replicate

let getElectronicDoorPlaces (state: State) =
    state.Board |> getFilteredPlaces (fun place -> (place.Tile = Tile.ClosedDoor || place.Tile = Tile.OpenDoor) && place.ElectronicMachine.IsSome)

let getCameraPlaces (state: State) =
    state.Board |> getFilteredPlaces (fun place -> (place.ElectronicMachine.IsSome && place.ElectronicMachine.Value.ComputerContent.HasCamera))

let getAvailableReplicationRecipes (state: State) =
    Replication.allRecipes |> List.filter (fun item -> state.AvailableReplicationRecipes.Contains item.Name)

let getCameraView (point: Point) (viewRadius: int) (state: State) =
    let viewArray = Array2D.create (viewRadius * 2 + 1) (viewRadius * 2 + 1) Place.EmptyPlace
    let xModifier = if (point.X - viewRadius) < 0 then abs (point.X - viewRadius) else 0
    let yModifier = if (point.Y - viewRadius) < 0 then abs (point.Y - viewRadius) else 0
    let width = if xModifier > 0 then (viewRadius * 2 + 1) - xModifier else min (viewRadius * 2 + 1) (boardWidth - point.X)
    let height = if yModifier > 0 then (viewRadius * 2 + 1) - yModifier else  min (viewRadius * 2 + 1) (boardHeight - point.Y)
    Array2D.blit state.Board.Places ((point.X - viewRadius) + xModifier) ((point.Y - viewRadius) + yModifier) viewArray (0 + xModifier) (0 + yModifier) width height
    viewArray

let operateComputer (computerPoint: Point option) (electronicMachine: ElectronicMachine) (state: State) =
    
    let maxSelectableNumericItems = 9 //digits from 1 to 9

    let canReplicate (recipe: ReplicationRecipe) (state: State) =
        if recipe.RequiredResources.Iron <= state.Player.Iron
            && recipe.RequiredResources.Gold <= state.Player.Gold
            && recipe.RequiredResources.Uranium <= state.Player.Uranium
        then true else false
            
    let createDisplayContent (content: ComputerContent) (currentNav: ComputerNavigation*int) (state: State) =
        let getListSlice n (list: 'a list) =
            if list.Length > maxSelectableNumericItems then
                let skipped = list |> Seq.ofList |> Seq.skip (n*maxSelectableNumericItems) |> Seq.toList
                if skipped.Length > maxSelectableNumericItems then
                    skipped |> Seq.ofList |> Seq.take maxSelectableNumericItems |> Seq.toList
                else
                    skipped
            else
                list

        let nav, itemNr = currentNav

        let builder = ScreenContentBuilder(Screen.boardFrameSize.Width, ColorTheme.ComputerTheme)

        builder.AddString(content.ComputerName)
        builder.AddString("")
        let back = [""] @ ["b. back"] @ [""]
        let selectBack = ("b",". back")
        let next = ["n. next"]
        let prev = ["p. previous"]
        let prevNext (n: int) (itemsNum: int) =
            let result = (if n > 0 then "p. previous     " else "") + (if itemsNum - (n * maxSelectableNumericItems) > maxSelectableNumericItems then "n. next" else "")
            if result.Length > 0 then [""] @ [result] else []
        let selectPrevNext (n: int) (itemsNum: int) =
            let result = (if n > 0 then [("p",". previous")] else []) @ (if itemsNum - (n * maxSelectableNumericItems) > maxSelectableNumericItems then [("n",". next")] else [])
            result
            
        let replicate (recipe: ReplicationRecipe) (state: State) (builder: ScreenContentBuilder) =
             if canReplicate recipe state
                then
                    builder.AddSelectables false [("r",". replicate")]
                else
                    builder.AddString "You don't have enough resources to replicate this item."

        let addPageableSelectableItems (itemNr: int) (descriptionFunction: 'a -> string) (builder: ScreenContentBuilder) (items: 'a list) =
            items |> getListSlice itemNr |> List.mapi (fun i item -> ( (i + 1).ToString(), ". " + (descriptionFunction item) )) |> builder.AddSelectables false
            builder.AddEmptyLine()
            (selectPrevNext itemNr items.Length) |> builder.AddSelectables true
            builder.AddEmptyLineIfPreviousNotEmpty()
            builder.AddSelectables false [selectBack]

        let mainContent =
            match nav with
            | MainMenu ->
                let result =
                    if content.Notes.Length > 0 then ["Notes"] else []
                    @
                    if content.CanOperateDoors then ["Electronic doors"] else []
                    @
                    if content.CanOperateCameras && (getCameraPlaces state).Length > 0 then ["Cameras"] else []
                    @
                    if content.CanReplicate then ["Replicate"] else []
                result |> List.mapi (fun i item -> ((i+1).ToString(),(". " + item))) |> builder.AddSelectables false
            | Notes ->
                content.Notes |> addPageableSelectableItems itemNr (fun note -> note.Topic) builder
            | Note ->
                builder.AddString content.Notes.[itemNr].Topic
                builder.AddSeparator()
                builder.AddString content.Notes.[itemNr].Content
                builder.AddEmptyLine()
                builder.AddSelectables false [selectBack]
            | Doors ->
                getElectronicDoorPlaces state
                |> addPageableSelectableItems itemNr (fun doorPlace ->
                    (fst doorPlace).ToString() + " " + (snd doorPlace).ElectronicMachine.Value.ComputerContent.ComputerName
                    + " - " + (if (snd doorPlace).Tile = ClosedDoor then "closed" else "open")) builder
            | Door ->
                let doorPoint, doorPlace = (getElectronicDoorPlaces state).[itemNr]
                builder.AddString(doorPoint.ToString() + " " + doorPlace.ElectronicMachine.Value.ComputerContent.ComputerName + " - " + (if doorPlace.Tile = ClosedDoor then "closed" else "open"))
                builder.AddEmptyLine()
                builder.AddSelectables false [("1",". " + (if doorPlace.Tile = ClosedDoor then "Open" else "Close"))]
                builder.AddEmptyLine()
                builder.AddSelectables false [selectBack]
            | Cameras ->
                getCameraPlaces state
                |> addPageableSelectableItems itemNr (fun cameraPlace -> (fst cameraPlace).ToString() + " " + (snd cameraPlace).ElectronicMachine.Value.ComputerContent.ComputerName + " camera") builder
            | Camera ->
                let cameraPoint, cameraPlace = (getCameraPlaces state).[itemNr]
                builder.AddString(cameraPoint.ToString() + " " + cameraPlace.ElectronicMachine.Value.ComputerContent.ComputerName + " camera")
                builder.AddEmptyLine()
                builder.AddPlacesArray(getCameraView cameraPoint 4 state)
                builder.AddEmptyLine()
                builder.AddSelectables false [selectBack]
            | Replication ->
                getAvailableReplicationRecipes state
                |> addPageableSelectableItems itemNr (fun recipe -> recipe.Name) builder
            | ReplicationItem ->
                let recipe = (getAvailableReplicationRecipes state).[itemNr]
                builder.AddString recipe.Name
                builder.AddSeparator ()
                builder.AddString "Required resources:"
                if recipe.RequiredResources.Iron > 0 then builder.AddString ("Iron: " + recipe.RequiredResources.Iron.ToString())
                if recipe.RequiredResources.Gold > 0 then builder.AddString ("Gold: " + recipe.RequiredResources.Gold.ToString())
                if recipe.RequiredResources.Uranium > 0 then builder.AddString ("Uranium: " + recipe.RequiredResources.Uranium.ToString())
                builder.AddEmptyLine ()
                replicate recipe state builder
                builder.AddSelectables false [selectBack]
        builder.AddEmptyLineIfPreviousNotEmpty ()
        builder.AddSelectables false [("Esc"," - to exit the computer")]
        builder

    let keyToComputerNavAndCommand (keyInfo: ConsoleKeyInfo) (currentNav: ComputerNavigation*int) (content: ComputerContent) =
        let nav, itemNr = currentNav
        let canGoNext n itemsNr =
            if itemsNr - (n * maxSelectableNumericItems) > maxSelectableNumericItems then true else false
        match nav with
        | MainMenu ->
            let items =
                (if content.Notes.Length > 0 then [ComputerNavigation.Notes] else [])
                @ (if content.CanOperateDoors then [ComputerNavigation.Doors] else [])
                @ (if content.CanOperateCameras && (getCameraPlaces state).Length > 0 then [ComputerNavigation.Cameras] else [])
                @ (if content.CanReplicate then [ComputerNavigation.Replication] else [])
            (
            match keyInfo with
            | Keys ['1';'2';'3';'4';'5';'6';'7';'8';'9'] ->
                let number = (Int32.Parse (keyInfo.KeyChar.ToString())) - 1
                if number < items.Length then (items.[number],0) else (nav, itemNr)
            | _ -> (nav, itemNr)
            , None)
        | Notes ->
            (
            match keyInfo with
            | Keys ['b'] -> (MainMenu,0)
            | Keys ['1';'2';'3';'4';'5';'6';'7';'8';'9'] ->
                let number = (Int32.Parse (keyInfo.KeyChar.ToString())) - 1 + (maxSelectableNumericItems * itemNr)
                if number < content.Notes.Length then (Note,number) else (nav, itemNr)
            | Keys ['n'] ->
                if canGoNext itemNr content.Notes.Length then (Notes,itemNr + 1) else (nav, itemNr)
            | Keys ['p'] ->
                if itemNr > 0 then (Notes,itemNr - 1) else (nav, itemNr)
            | _ -> (nav, itemNr)
            , None)
        | Note ->
            (
            match keyInfo with
            | Keys ['b'] -> (Notes,0)
            | _ -> (nav, itemNr)
            , None)
        | Doors ->
            let doorPlaces = getElectronicDoorPlaces state
            (
            match keyInfo with
            | Keys ['b'] -> (MainMenu,0)
            | Keys ['1';'2';'3';'4';'5';'6';'7';'8';'9'] ->
                let number = (Int32.Parse (keyInfo.KeyChar.ToString())) - 1 + (maxSelectableNumericItems * itemNr)
                if number < doorPlaces.Length then (Door,number) else (nav, itemNr)
            | Keys ['n'] ->
                if canGoNext itemNr doorPlaces.Length then (Doors,itemNr + 1) else (nav, itemNr)
            | Keys ['p'] ->
                if itemNr > 0 then (Doors,itemNr - 1) else (nav, itemNr)
            | _ -> (nav, itemNr)
            , None)
        | Door ->
            match keyInfo with
            | Keys ['b'] -> ((Doors,0), None)
            | Keys ['1';'2';'3';'4';'5';'6';'7';'8';'9'] ->
                let doorPoint, doorPlace = (getElectronicDoorPlaces state).[itemNr]
                if (doorPlace.Tile = Tile.ClosedDoor) then ((nav, itemNr), OpenDoor) else ((nav, itemNr), CloseDoor)
            | _ -> ((nav, itemNr), None)
        | Cameras ->
            let cameraPlaces = getCameraPlaces state
            (
            match keyInfo with
            | Keys ['b'] -> (MainMenu,0)
            | Keys ['1';'2';'3';'4';'5';'6';'7';'8';'9'] ->
                let number = (Int32.Parse (keyInfo.KeyChar.ToString())) - 1 + (maxSelectableNumericItems * itemNr)
                if number < cameraPlaces.Length then (Camera,number) else (nav, itemNr)
            | Keys ['n'] ->
                if canGoNext itemNr cameraPlaces.Length then (Cameras,itemNr + 1) else (nav, itemNr)
            | Keys ['p'] ->
                if itemNr > 0 then (Cameras,itemNr - 1) else (nav, itemNr)
            | _ -> (nav, itemNr)
            , None)
        | Camera ->
            match keyInfo with
            | Keys ['b'] -> ((Cameras,0), None)
            | _ -> ((nav, itemNr), None)
        | Replication ->
            let recipes = getAvailableReplicationRecipes state
            (
            match keyInfo with
            | Keys ['b'] -> (MainMenu,0)
            | Keys ['1';'2';'3';'4';'5';'6';'7';'8';'9'] ->
                let number = (Int32.Parse (keyInfo.KeyChar.ToString())) - 1 + (maxSelectableNumericItems * itemNr)
                if number < recipes.Length then (ReplicationItem,number) else (nav, itemNr)
            | Keys ['n'] ->
                if canGoNext itemNr recipes.Length then (Replication,itemNr + 1) else (nav, itemNr)
            | Keys ['p'] ->
                if itemNr > 0 then (Replication,itemNr - 1) else (nav, itemNr)
            | _ -> (nav, itemNr)
            , None)
        | ReplicationItem ->
            let recipe = (getAvailableReplicationRecipes state).[itemNr]
            match keyInfo with
            | Keys ['b'] -> ((Replication,0), None)
            | Keys ['r'] -> if canReplicate recipe state then ((Replication,0),Replicate) else ((nav, itemNr), None)
            | _ -> ((nav, itemNr), None)
    
    let performComputerCommand (computerPoint: Point option) (currentNav: ComputerNavigation*int) (command: ComputerCommand) (state : State) =
        let nav, itemNr = currentNav
        match command with
        | OpenDoor | CloseDoor ->
            let doorPoint, doorPlace = (getElectronicDoorPlaces state).[itemNr]
            let newBoard =
                state.Board
                |> Board.modify doorPoint
                    (
                    fun (place : Place) -> 
                        match place.Tile with
                        | Tile.OpenDoor -> {place with Tile = Tile.ClosedDoor}  
                        | Tile.ClosedDoor -> {place with Tile = Tile.OpenDoor}
                        | _ -> place)
            let result = { state with Board = newBoard }
            result |> Turn.next
        | Replicate ->
            let recipe = (getAvailableReplicationRecipes state).[itemNr]
            state.Player.Iron <- state.Player.Iron - recipe.RequiredResources.Iron
            state.Player.Gold <- state.Player.Gold - recipe.RequiredResources.Gold
            state.Player.Uranium <- state.Player.Uranium - recipe.RequiredResources.Uranium
            let item = recipe.ResultItem
            let compPlace = state.Board.Places.[computerPoint.Value.X,computerPoint.Value.Y]
            let updatedPlace = { compPlace with Items = compPlace.Items @ [item] }
            state.Board.Places.[computerPoint.Value.X,computerPoint.Value.Y] <- updatedPlace
            state |> Turn.next
        | _ ->
            ()

    let rec loop (computerPoint: Point option) (content: ComputerContent) (nav: ComputerNavigation*int) (state : State) =
        displayComputerScreen (createDisplayContent content nav state)
        let keyInfo = System.Console.ReadKey(true)
        match keyInfo with 
        | Key ConsoleKey.Escape -> state
        | _ ->
            let newNav, command = keyToComputerNavAndCommand keyInfo nav content
            state |> (performComputerCommand computerPoint newNav command)
            let newState = State.get()
            loop computerPoint content newNav newState
    let comp = electronicMachine.ComputerContent
    loop computerPoint comp (MainMenu,0) state 