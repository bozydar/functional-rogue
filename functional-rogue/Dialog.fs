[<RequireQualifiedAccessAttribute>]
module Dialog

open System

type Dialog = Widget list
and Widget = 
    | Title of string
    | Label of string
    | Menu of string * (Item list)
    | Raw of DecoratedText
    | Textbox of string
and Item = 
    | Item of char * string * string
    | Subdialog of char * string * Dialog
    | CheckItem of char * string * string
and DecoratedText = {
    Text : string;
    BGColor : ConsoleColor;
    FGColor : ConsoleColor;
}
and TextOrDialog =
    | Text of string

let newDecoratedText text bg fg = { Text = text; BGColor = bg; FGColor = fg }

type decoratedTexts = DecoratedText list        

let (>>=) x f =
    (fun s0 ->
        let a,s = x s0    
        f a s)
let returnS a = (fun s -> a, s)

type StateBuilder() =
    member m.Delay(f) = f()
    member m.Bind(x, f) = x >>= f
    member m.Return a = returnS a
    member m.ReturnFrom(f) = f

let state = new StateBuilder()     

let getState = (fun s -> s, s)
let setState s = (fun _ -> (),s) 

let runState m s = m s |> fst

(*
See Game module variable d1 and d2
FUTURE:

let d1 = dialog {
    do! title("Some title")
    do! label("Do you want to play")  
    yield menu("ynResult") {
        do! item('y', "Yes", "1");
        do! item('n', "No", "0");
    }
    do! label("Enter your name")  
    yield textbox("name")
}
*)