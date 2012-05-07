[<RequireQualifiedAccessAttribute>]
module Predefined.Help

open System.Text.RegularExpressions
open Dialog

let commands = [
        "Up, 8", "Go north";
        "Down, 2", "Go south";
        "Left, 4", "Go west";
        "Right, 6", "Go east";
        "7", "Go north-west";
        "1", "Go south-west";
        "3", "Go south-east";
        "9", "Go north-east";
        "Comma", "Take";
        "i", "Show items";
        "Escape", "Save & Quit";
        "o", "Open/Close doors";
        "e", "Show equipment";
        "E", "Eat";
        "m", "Show list of messages";
        "h", "Harvest ore";
        "W", "Wear shield, armory or weapon";
        "T", "Take off shield, armory or weapon";
        ">", "Go down or start explore area";
        "<", "Go up";
        "d", "Drop item";
        "l", "Look";
        "U", "Use object which is on the board";
        "u", "Use item from your inventory";
        "O", "???";
        "Ctrl-P", "Pour liquid";
        "?", "This help";
    ]

let _commands = new DecoratedTextBuilder() => BG.Blue => FG.Red => "Up, 8"

(*
let _commands = BG.Red => FG.Black => "Up, 8" |> 


let private expression = new Regex(@"\[(?<command>.*?)\]\{(?<content>.*[^\\]?)\}", RegexOptions.Multiline ||| RegexOptions.Compiled)

let parseCommand (command : string) = 
    if command.StartsWith("#") then
        let hex = command.Substring(1)
        let fg = hex

let parse (text : string) = seq {
    for line in text.Split("\n") do
        for item in expression.Matches(text) do
            item.Groups.["command"]
    }

let text = ["[#AA]{Up, 8} [#BB]{ - } Go north"]
let text = ["[#AA]{Up, 8} [#BB]{ - } Go north"]
*)