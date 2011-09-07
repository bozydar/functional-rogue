module Computers

open System
open State
open Screen

let operateComputer state =
    let createDisplayContent =
        let header = "Welcome"
        let esc = "Hit Esc to exit"
        [header] @ [""] @ [esc]

    let rec loop (state : State) =
        //setCursorPositionOnBoard current state
        let keyInfo = System.Console.ReadKey(true)
        match keyInfo with 
        | Keys [ConsoleKey.UpArrow; '8'] -> loop (state) //Up
        | Keys [ConsoleKey.DownArrow; '2'] -> loop (state) //Down        
        | Keys [ConsoleKey.LeftArrow; '4'] -> loop (state) //Left            
        | Keys [ConsoleKey.RightArrow; '6'] -> loop (state) //Right
        | Key ConsoleKey.Escape -> state
        | _ ->
            displayComputerScreen createDisplayContent
            loop state
    displayComputerScreen createDisplayContent
    loop state 