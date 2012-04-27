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

type Dialog (sequence : seq<Widget>) = 
    interface seq<Widget> with
        member this.GetEnumerator () =
            sequence.GetEnumerator ()
        member this.GetEnumerator () : System.Collections.IEnumerator  =
            (sequence :> System.Collections.IEnumerable).GetEnumerator ()

    with static member (+) (left : Dialog, right : Dialog) = new Dialog (Seq.append left right)

and Widget = 
    | Title of string
    | Label of string
    | Action of Input * string * string * string
    | Option of Input * string * string * (OptionItem list)
//    | Action of char * string * string * string
//    | Option of char * string * string * (OptionItem list)
    | Subdialog of Input * string * Dialog
    | Raw of DecoratedText
    | Textbox of Input * string
and OptionItem = string * string

and DecoratedText = {
    Text : string;
    BGColor : ConsoleColor;
    FGColor : ConsoleColor;
}

let newDecoratedText text bg fg = { Text = text; BGColor = bg; FGColor = fg }

type decoratedTexts = DecoratedText list        