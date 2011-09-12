module Computers

open System
open State
open Screen
open Board

type ComputerNavigation =
    | MainMenu
    | Notes
    | Note
    | Doors
    | Door

type ComputerCommand =
    | None
    | OpenDoor
    | CloseDoor

let getelectronicDoorPlaces (state: State) =
    state.Board |> getFilteredPlaces (fun place -> (place.Tile = Tile.ClosedDoor || place.Tile = Tile.OpenDoor) && place.ElectronicMachine.IsSome)

let operateComputer (electronicMachine: ElectronicMachine) (state: State) =
    let createDisplayContent (content: ComputerContent) (currentNav: ComputerNavigation*int) =
        let nav, itemNr = currentNav
        let compName = content.ComputerName
        let back = [""] @ ["b. back"] @ [""]
        let mainContent =
            match nav with
            | MainMenu ->
                let result =
                    if content.Notes.Length > 0 then ["Notes"] else []
                    @
                    if content.CanOperateDoors then ["Electronic doors"] else []
                result |> List.mapi (fun i item -> (i+1).ToString() + ". " + item)
            | Notes ->
                (content.Notes |> List.mapi (fun x item -> (x + 1).ToString() + ". " + item.Topic))
                @ back
            | Note ->
                [content.Notes.[itemNr].Topic] @ [""] @
                [content.Notes.[itemNr].Content]
                @ back
            | Doors ->
                let doorPlaces = getelectronicDoorPlaces state
                (doorPlaces |> List.mapi (fun x item ->
                    (x + 1).ToString() + ". (" + (fst item).X.ToString() + "," + (fst item).Y.ToString() + ") " + (snd item).ElectronicMachine.Value.ComputerContent.ComputerName + " - " + (if (snd item).Tile = ClosedDoor then "closed" else "open")))
                @ back
            | Door ->
                let doorPoint, doorPlace = (getelectronicDoorPlaces state).[itemNr]
                [doorPoint.ToString() + " " + doorPlace.ElectronicMachine.Value.ComputerContent.ComputerName + " - " + (if doorPlace.Tile = ClosedDoor then "closed" else "open")]
                @ [""] @ ["1. " + (if doorPlace.Tile = ClosedDoor then "Open" else "Close")] @ [""]
                @ back
            | _ -> []
        let esc = "Hit Esc to exit"
        [compName] @ [""] @ mainContent @ [""] @ [esc]

    let keyToComputerNavAndCommand (keyInfo: ConsoleKeyInfo) (currentNav: ComputerNavigation*int) (content: ComputerContent) =
        let nav, itemNr = currentNav
        match nav with
        | MainMenu ->
            let items =
                (if content.Notes.Length > 0 then [ComputerNavigation.Notes] else [])
                @ (if content.CanOperateDoors then [ComputerNavigation.Doors] else [])
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
                let number = (Int32.Parse (keyInfo.KeyChar.ToString())) - 1
                if number < content.Notes.Length then (Note,number) else (nav, itemNr)
            | _ -> (nav, itemNr)
            , None)
        | Note ->
            (
            match keyInfo with
            | Keys ['b'] -> (Notes,0)
            | _ -> (nav, itemNr)
            , None)
        | Doors ->
            (
            match keyInfo with
            | Keys ['b'] -> (MainMenu,0)
            | Keys ['1';'2';'3';'4';'5';'6';'7';'8';'9'] ->
                let doorPlaces = getelectronicDoorPlaces state
                let number = (Int32.Parse (keyInfo.KeyChar.ToString())) - 1
                if number < doorPlaces.Length then (Door,number) else (nav, itemNr)
            | _ -> (nav, itemNr)
            , None)
        | Door ->
            match keyInfo with
            | Keys ['b'] -> ((Doors,0), None)
            | Keys ['1';'2';'3';'4';'5';'6';'7';'8';'9'] ->
                let doorPoint, doorPlace = (getelectronicDoorPlaces state).[itemNr]
                if (doorPlace.Tile = Tile.ClosedDoor) then ((nav, itemNr), OpenDoor) else ((nav, itemNr), CloseDoor)
            | _ -> ((nav, itemNr), None)
        | _ -> ((MainMenu, 0),None)
    
    let performComputerCommand (currentNav: ComputerNavigation*int) (command: ComputerCommand) (state : State) =
        let nav, itemNr = currentNav
        match command with
        | OpenDoor | CloseDoor ->
            let doorPoint, doorPlace = (getelectronicDoorPlaces state).[itemNr]
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
        | _ ->
            state

    let rec loop (content: ComputerContent) (nav: ComputerNavigation*int) (state : State) =
        displayComputerScreen (createDisplayContent content nav)
        let keyInfo = System.Console.ReadKey(true)
        match keyInfo with 
        | Key ConsoleKey.Escape -> state
        | _ ->
            let newNav, command = keyToComputerNavAndCommand keyInfo nav content
            let newState = state |> (performComputerCommand newNav command)
            State.set newState
            loop content newNav newState
    let comp = electronicMachine.ComputerContent
    loop comp (MainMenu,0) state 