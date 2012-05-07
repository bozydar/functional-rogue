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


type DecoratedText = {
    Text : string;
    BGColor : ConsoleColor;
    FGColor : ConsoleColor;
}

type BG = { Color : ConsoleColor }
    with 
        static member Black = { Color = ConsoleColor.Black}
        static member Blue = { Color = ConsoleColor.Blue}
        static member Cyan = { Color = ConsoleColor.Cyan}
        static member DarkBlue = { Color = ConsoleColor.DarkBlue}
        static member DarkCyan = { Color = ConsoleColor.DarkCyan}
        static member DarkGreen = { Color = ConsoleColor.DarkGreen}
        static member DarkMagenta = { Color = ConsoleColor.DarkMagenta}
        static member DarkRed = { Color = ConsoleColor.DarkRed}
        static member DarkYellow = { Color = ConsoleColor.DarkYellow}
        static member Gray = { Color = ConsoleColor.Gray}
        static member Green = { Color = ConsoleColor.Green}
        static member Magenta = { Color = ConsoleColor.Magenta}
        static member Red = { Color = ConsoleColor.Red}
        static member White = { Color = ConsoleColor.White}
        static member Yellow = { Color = ConsoleColor.Yellow}

type FG = { Color : ConsoleColor }
    with 
        static member Black = { Color = ConsoleColor.Black}
        static member Blue = { Color = ConsoleColor.Blue}
        static member Cyan = { Color = ConsoleColor.Cyan}
        static member DarkBlue = { Color = ConsoleColor.DarkBlue}
        static member DarkCyan = { Color = ConsoleColor.DarkCyan}
        static member DarkGreen = { Color = ConsoleColor.DarkGreen}
        static member DarkMagenta = { Color = ConsoleColor.DarkMagenta}
        static member DarkRed = { Color = ConsoleColor.DarkRed}
        static member DarkYellow = { Color = ConsoleColor.DarkYellow}
        static member Gray = { Color = ConsoleColor.Gray}
        static member Green = { Color = ConsoleColor.Green}
        static member Magenta = { Color = ConsoleColor.Magenta}
        static member Red = { Color = ConsoleColor.Red}
        static member White = { Color = ConsoleColor.White}
        static member Yellow = { Color = ConsoleColor.Yellow}



type DecoratedTextBuilder (text : string, background : BG, foreground : FG) = 
    member this.Text = text
    member this.Background = background
    member this.Foreground = foreground

    new() = DecoratedTextBuilder("", BG.Black, FG.Gray)
        

    static member (=>) (me : DecoratedTextBuilder, text : string) =
        DecoratedTextBuilder(text, me.Background, me.Foreground)

    static member (=>) (me : DecoratedTextBuilder, foreground : FG) =
        DecoratedTextBuilder(me.Text, me.Background, foreground)

    static member (=>) (me : DecoratedTextBuilder, background : BG) =
        DecoratedTextBuilder(me.Text, background, me.Foreground)

    member this.ToDecoratedText = { Text = this.Text; BGColor = this.Background.Color; FGColor = this.Foreground.Color }


let newDecoratedText text bg fg = { Text = text; BGColor = bg; FGColor = fg }

type Dialog (sequence : seq<Widget>) = 
    interface seq<Widget> with
        member this.GetEnumerator () =
            sequence.GetEnumerator ()
        member this.GetEnumerator () : System.Collections.IEnumerator  =
            (sequence :> System.Collections.IEnumerable).GetEnumerator ()

    with static member (+) (left : Dialog, right : Dialog) = new Dialog (Seq.append left right)
and Widget = 
    | CR
    | Title of string
    | Label of string
    | Action of Input * string * string * string
    | Option of Input * string * string * (OptionItem list)
    | Subdialog of Input * string * Dialog
    | Raw of DecoratedText
    | Textbox of Input * string
with static member newDecoratedText (text, bg, fg) =
        Widget.Raw(newDecoratedText text bg fg)
and OptionItem = string * string
    
type decoratedTexts = DecoratedText list        
