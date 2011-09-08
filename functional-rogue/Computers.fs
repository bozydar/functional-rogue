module Computers

open System
open State
open Screen

type ComputerNavigation =
    | MainMenu
    | Notes
    | Note

type ComputerNote = {
    Topic : string;
    Content : string;
}

type ComputerContent = {
    ComputerName : string;
    Notes : ComputerNote list
}

let getTestComputerContent =
    let note1 = { Topic = "Test note 1"; Content = "This is my test note number one." }
    let note2 = { Topic = "Test note 2"; Content = "This is my test note number two. This one is longer." }
    { ComputerName = "TestComp"; Notes = [note1; note2] }

let operateComputer state =
    let createDisplayContent (content: ComputerContent) (currentNav: ComputerNavigation*int) =
        let nav, itemNr = currentNav
        let compName = content.ComputerName
        let back = [""] @ ["b. back"] @ [""]
        let mainContent =
            match nav with
            | MainMenu ->
                if content.Notes.Length > 0 then ["1. Notes"] else []
            | Notes ->
                (content.Notes |> List.mapi (fun x item -> (x + 1).ToString() + ". " + item.Topic))
                @ back
            | Note ->
                [content.Notes.[itemNr].Topic] @ [""] @
                [content.Notes.[itemNr].Content]
                @ back
            | _ -> []
        let esc = "Hit Esc to exit"
        [compName] @ [""] @ mainContent @ [""] @ [esc]

    let keyToComputerNav (keyInfo: ConsoleKeyInfo) (currentNav: ComputerNavigation*int) (content: ComputerContent) =
        let nav, itemNr = currentNav
        match nav with
        | MainMenu ->
            let items = (if content.Notes.Length > 0 then [ComputerNavigation.Notes] else [])
            match keyInfo with
            | Keys ['1'] -> (items.[0],0)
            | _ -> (nav, itemNr)
        | Notes ->
            match keyInfo with
            | Keys ['b'] -> (MainMenu,0)
            | Keys ['1'] -> (Note,0)
            | Keys ['2'] -> (Note,1)
            | _ -> (nav, itemNr)
        | Note ->
            match keyInfo with
            | Keys ['b'] -> (Notes,0)
            | _ -> (nav, itemNr)
        | _ -> (MainMenu, 0)

    let rec loop (content: ComputerContent) (nav: ComputerNavigation*int) (state : State) =
        let keyInfo = System.Console.ReadKey(true)
        match keyInfo with 
        | Key ConsoleKey.Escape -> state
        | _ ->
            let newNav = keyToComputerNav keyInfo nav content
            displayComputerScreen (createDisplayContent content newNav)
            loop content newNav state
    let comp = getTestComputerContent
    displayComputerScreen (createDisplayContent comp (MainMenu,0))
    loop comp (MainMenu,0) state 