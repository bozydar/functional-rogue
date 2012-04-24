[<AutoOpen>]
module Keyboard

open System

let (|Key|_|) (charOrConsoleKey : obj) (input : ConsoleKeyInfo) =
    match charOrConsoleKey with
    | :? char as thisChar when thisChar = input.KeyChar -> Some()
    | :? ConsoleKey as thisKey when thisKey = input.Key -> Some()
    | _ -> None

let (|Keys|_|) (charsOrConsoleKeys : seq<obj>) (input : ConsoleKeyInfo) =
    if charsOrConsoleKeys |> Seq.exists (fun item -> match input with | Key item -> true | _ -> false) then Some() else None
