module Computers

open System
open State
open Screen
open Board
open Replication
open System.Drawing

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

//let getCameraView (point: Point) (viewRadius: int) (state: State) =
//    //state.Board.p
//    let viewArray = Array2D.create (viewRadius * 2 + 1) (viewRadius * 2 + 1) Place.EmptyPlace
//    let places = viewArray |> Seq.cast<Place>

let operateComputer (computerPoint: Point option) (electronicMachine: ElectronicMachine) (state: State) =
    
    let canReplicate (recipe: ReplicationRecipe) (state: State) =
        if recipe.RequiredResources.Iron <= state.Player.Iron
            && recipe.RequiredResources.Gold <= state.Player.Gold
            && recipe.RequiredResources.Uranium <= state.Player.Uranium
        then true else false
            
    let createDisplayContent (content: ComputerContent) (currentNav: ComputerNavigation*int) =
        let getListSlice n (list: 'a list) =
            if list.Length > 9 then
                let skipped = list |> Seq.ofList |> Seq.skip (n*9) |> Seq.toList
                if skipped.Length > 9 then
                    skipped |> Seq.ofList |> Seq.take 9 |> Seq.toList
                else
                    skipped
            else
                list

        let nav, itemNr = currentNav
        let compName = content.ComputerName
        let back = [""] @ ["b. back"] @ [""]
        let next = ["n. next"]
        let prev = ["p. previous"]
        let prevNext (n: int) (itemsNum: int) =
            let result = (if n > 0 then "p. previous     " else "") + (if itemsNum - (n * 9) > 9 then "n. next" else "")
            if result.Length > 0 then [""] @ [result] else []
        let replicate (recipe: ReplicationRecipe) (state: State) =
             if canReplicate recipe state
                then
                    ["r. replicate"]
                else
                    ["You don't have enough resources to replicate this item."]
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
                result |> List.mapi (fun i item -> (i+1).ToString() + ". " + item)
            | Notes ->
                (content.Notes |> getListSlice itemNr |> List.mapi (fun x item -> (x + 1).ToString() + ". " + item.Topic))
                @ prevNext itemNr content.Notes.Length
                @ back
            | Note ->
                [content.Notes.[itemNr].Topic] @ [""] @
                [content.Notes.[itemNr].Content]
                @ back
            | Doors ->
                let doorPlaces = getElectronicDoorPlaces state
                (doorPlaces |> List.mapi (fun x item ->
                    (x + 1).ToString() + ". (" + (fst item).X.ToString() + "," + (fst item).Y.ToString() + ") " + (snd item).ElectronicMachine.Value.ComputerContent.ComputerName + " - " + (if (snd item).Tile = ClosedDoor then "closed" else "open")))
                @ back
            | Door ->
                let doorPoint, doorPlace = (getElectronicDoorPlaces state).[itemNr]
                [doorPoint.ToString() + " " + doorPlace.ElectronicMachine.Value.ComputerContent.ComputerName + " - " + (if doorPlace.Tile = ClosedDoor then "closed" else "open")]
                @ [""] @ ["1. " + (if doorPlace.Tile = ClosedDoor then "Open" else "Close")] @ [""]
                @ back
            | Cameras ->
                let cameraPlaces = getCameraPlaces state
                (cameraPlaces |> getListSlice itemNr |> List.mapi (fun x item ->
                    (x + 1).ToString() + ". " + (fst item).ToString() + " " + (snd item).ElectronicMachine.Value.ComputerContent.ComputerName + " camera"))
                @ back
            | Camera ->
                let cameraPoint, cameraPlace = (getCameraPlaces state).[itemNr]
                [cameraPoint.ToString() + " " + cameraPlace.ElectronicMachine.Value.ComputerContent.ComputerName + " camera"]
                @ [""] @ [" TODO: CAMERA VIEW HERE! "] @ [""]
                @ back
            | Replication ->
                let recipes = getAvailableReplicationRecipes state
                (recipes |> getListSlice itemNr |> List.mapi (fun x item ->
                    (x + 1).ToString() + ". " + item.Name ))
                @ back
            | ReplicationItem ->
                let recipe = (getAvailableReplicationRecipes state).[itemNr]
                [recipe.Name]
                @ [""] @ ["Required resources:"]
                @ if recipe.RequiredResources.Iron > 0 then ["Iron: " + recipe.RequiredResources.Iron.ToString()] else []
                @ if recipe.RequiredResources.Gold > 0 then ["Gold: " + recipe.RequiredResources.Gold.ToString()] else []
                @ if recipe.RequiredResources.Uranium > 0 then ["Uranium: " + recipe.RequiredResources.Uranium.ToString()] else []
                @ [""] @ (replicate recipe state)
                @ back
            | _ -> []
        let esc = "Hit Esc to exit"
        [compName] @ [""] @ mainContent @ [""] @ [esc]

    let keyToComputerNavAndCommand (keyInfo: ConsoleKeyInfo) (currentNav: ComputerNavigation*int) (content: ComputerContent) =
        let nav, itemNr = currentNav
        let canGoNext n itemsNr =
            if itemsNr - (n * 9) > 9 then true else false
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
                let number = (Int32.Parse (keyInfo.KeyChar.ToString())) - 1 + (9 * itemNr)
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
                let number = (Int32.Parse (keyInfo.KeyChar.ToString())) - 1 + (9 * itemNr)
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
                let number = (Int32.Parse (keyInfo.KeyChar.ToString())) - 1 + (9 * itemNr)
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
                let number = (Int32.Parse (keyInfo.KeyChar.ToString())) - 1 + (9 * itemNr)
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
        | _ -> ((MainMenu, 0),None)
    
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
            { state with Board = newBoard }
        | Replicate ->
            let recipe = (getAvailableReplicationRecipes state).[itemNr]
            state.Player.Iron <- state.Player.Iron - recipe.RequiredResources.Iron
            state.Player.Gold <- state.Player.Gold - recipe.RequiredResources.Gold
            state.Player.Uranium <- state.Player.Uranium - recipe.RequiredResources.Uranium
            let item = Items.createPredefinedItem recipe.ResultItem
            let compPlace = state.Board.Places.[computerPoint.Value.X,computerPoint.Value.Y]
            let updatedPlace = { compPlace with Items = compPlace.Items @ [item] }
            state.Board.Places.[computerPoint.Value.X,computerPoint.Value.Y] <- updatedPlace
            state
        | _ ->
            state

    let rec loop (computerPoint: Point option) (content: ComputerContent) (nav: ComputerNavigation*int) (state : State) =
        displayComputerScreen (createDisplayContent content nav)
        let keyInfo = System.Console.ReadKey(true)
        match keyInfo with 
        | Key ConsoleKey.Escape -> state
        | _ ->
            let newNav, command = keyToComputerNavAndCommand keyInfo nav content
            let newState = state |> (performComputerCommand computerPoint newNav command)
            State.set newState
            loop computerPoint content newNav newState
    let comp = electronicMachine.ComputerContent
    loop computerPoint comp (MainMenu,0) state 