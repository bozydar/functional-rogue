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


type DecoratedText = {
    Text : string;
    BGColor : ConsoleColor;
    FGColor : ConsoleColor;
}

let newDecoratedText text bg fg = { Text = text; BGColor = bg; FGColor = fg }

type IWidget = interface 
    abstract member Render : Result -> seq<DecoratedText>
end

type Dialog (sequence : seq<IWidget>) = 
    interface seq<IWidget> with
        member this.GetEnumerator () =
            sequence.GetEnumerator ()
        member this.GetEnumerator () : System.Collections.IEnumerator  =
            (sequence :> System.Collections.IEnumerable).GetEnumerator ()
    with 
        static member (+) (left : Dialog, right : Dialog) = new Dialog (Seq.append left right)

type Nothing () =  
    interface IWidget with
        member this.Render (args : Result) : seq<DecoratedText> = seq {
            yield newDecoratedText "" ConsoleColor.Black ConsoleColor.Black
            yield newDecoratedText "\n" ConsoleColor.Black ConsoleColor.Black
        }

type CR () = 
    interface IWidget with
        member this.Render (args : Result) : seq<DecoratedText> = seq {
            yield newDecoratedText "\n" ConsoleColor.Black ConsoleColor.Black
        }

type Text (text : string, background : ConsoleColor, foreground : ConsoleColor) =
    interface IWidget with
        member this.Render (args : Result) : seq<DecoratedText> = seq {
            yield newDecoratedText text  background foreground
        }

type Title (text : string) = 
    interface IWidget with
        member this.Render (args : Result) : seq<DecoratedText> = seq {
            yield newDecoratedText text  ConsoleColor.White ConsoleColor.Black
        }

type Label (text : string) = 
    interface IWidget with
        member this.Render (args : Result) : seq<DecoratedText> = seq {
            yield newDecoratedText text  ConsoleColor.Black ConsoleColor.Gray
        }

type Action (input : Input, text : string, resultName : string, resultValue : string) =
    interface IWidget with
        member this.Render (args : Result) : seq<DecoratedText> = seq {
            yield newDecoratedText (input.ToString() + " - " + text) ConsoleColor.Black ConsoleColor.White
        }

type Subdialog (input : Input, text : string, subDialog : Dialog) = 
    interface IWidget with
        member this.Render (args : Result) : seq<DecoratedText> = seq {
            yield newDecoratedText (input.ToString() + " - " + text) ConsoleColor.Black ConsoleColor.White
        }

type Option (input : Input, text : string, resultName : string, resultValue : string, optionItems : seq<string * string>) =
    interface IWidget with
        member this.Render (args : Result) : seq<DecoratedText> = seq {
            let dt1 = newDecoratedText (input.ToString() + " - " + text) ConsoleColor.Black ConsoleColor.White 
            yield dt1
            if args.ContainsKey(resultName) then
                let selectedItem = Seq.tryFind (fun item -> snd item = args.[resultName]) optionItems
                if selectedItem.IsSome then
                    yield newDecoratedText (fst selectedItem.Value) ConsoleColor.Black ConsoleColor.Gray
        }
      
type decoratedTexts = DecoratedText list        

