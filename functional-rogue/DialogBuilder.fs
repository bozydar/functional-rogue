module DialogBuilder 

open System



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



type DialogBuilder (background : BG, foreground : FG, ?dialog : Dialog.Dialog) = 
    member this.Background = background
    member this.Foreground = foreground
    member this.Dialog = if dialog.IsSome then dialog.Value else Dialog.Dialog([])

    new() = DialogBuilder(BG.Black, FG.Gray, new Dialog.Dialog([]))

    static member (=>) (me : DialogBuilder, text : string) =
        let widget = 
            if text = "\n" then 
                Dialog.CR()
            else
                Dialog.Widget.Raw (Dialog.newDecoratedText text me.Background.Color  me.Foreground.Color)
        DialogBuilder(me.Background, me.Foreground, me.Dialog + Dialog.Dialog([widget]))

    static member (=>) (left : DialogBuilder, right : FG) =
        DialogBuilder(left.Background, right, left.Dialog)

    static member (=>) (left : DialogBuilder, right : BG) =
        DialogBuilder(right, left.Foreground, left.Dialog)

    static member (=>) (left : DialogBuilder, right : DialogBuilder) =
        DialogBuilder(right.Background, right.Foreground, left.Dialog)