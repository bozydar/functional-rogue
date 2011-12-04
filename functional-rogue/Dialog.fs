[<RequireQualifiedAccessAttribute>]
module Dialog

open System

type Dialog = Widget list
and Widget = 
    | Title of string
    | Label of string
    | Menu of string * (Item list)
and Item = 
    | Item of char * string * string
    
type DecoratedText = {
    Text : string;
    BGColor : ConsoleColor;
    FGColor : ConsoleColor;
} 

let newDecoratedText text bg fg = { Text = text; BGColor = bg; FGColor = fg }

type decoratedTexts = DecoratedText list        

