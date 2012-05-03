[<AutoOpen>]
module Keyboard

open System

type Input = 
    | Char of char
    | Console of ConsoleKey
    | Many of list<Input>
    with override this.ToString() = 
            match this with
            | Char(item) -> item.ToString()
            | Console(item) -> item.ToString()
            | Many(items) -> Seq.fold (fun acc item -> acc + ", " + item.ToString()) "" items

let rec (|Input|_|) (charOrConsoleKey : Input) (input : ConsoleKeyInfo) =
    match charOrConsoleKey with
    | Char(item) when item = input.KeyChar -> Some()
    | Console(item) when item = input.Key -> Some()
    | Many(items) when items |> Seq.exists (fun item -> 
        match input with 
        | Input(item) -> true
        | _ -> false) -> Some()
    | _ -> None

let isKeyInput (charOrConsoleKey : Input) (keyInfo : ConsoleKeyInfo) =
    match keyInfo with
    | Input(charOrConsoleKey) -> true
    | _ -> false

let (|Key|_|) (charOrConsoleKey : obj) (input : ConsoleKeyInfo) =
    match charOrConsoleKey with
    | :? char as thisChar when isKeyInput (Input.Char thisChar) input -> Some()
    | :? ConsoleKey as thisKey when isKeyInput (Input.Console thisKey) input -> Some()
    | _ -> None

let (|Keys|_|) (charsOrConsoleKeys : list<obj>) (input : ConsoleKeyInfo) =
    if charsOrConsoleKeys |> List.exists (fun item -> match input with | Key item -> true | _ -> false) then Some() else None
