[<RequireQualifiedAccessAttribute>]
module Dialog

open System

type Result = HashMultiMap<string, string>

let replaceWith (left : Result, right : Result) =
    let values = left.Fold (fun key value acc -> (key, value) :: acc) [] 
    let result = new Result (values, HashIdentity.Structural)
    right.Iterate (fun key value -> if result.ContainsKey key then result.Replace (key, value) else result.Add (key, value)) 
    result

let newResult (items : seq<string * string>) : Result = new Result(items, HashIdentity.Structural)

let emptyResult = new Result ([], HashIdentity.Structural)

type Dialog = Widget list
and Widget = 
    | Title of string
    | Label of string
    | Action of char * string * string * string
    | Option of char * string * string * (OptionItem list)
    | Subdialog of char * string * Dialog
    | Raw of DecoratedText
    | Textbox of char * string
and OptionItem = 
    | OptionItem of string * string

and DecoratedText = {
    Text : string;
    BGColor : ConsoleColor;
    FGColor : ConsoleColor;
}

let newDecoratedText text bg fg = { Text = text; BGColor = bg; FGColor = fg }

type decoratedTexts = DecoratedText list        