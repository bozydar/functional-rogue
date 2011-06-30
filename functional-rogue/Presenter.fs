module Presenter

open System
open System.Drawing
open Utils

let private width = 79
let private height = 24


type LocatedChar = {
    Position: Point;
    Char: Char
}

type ScreenState = {
    Old: char [,]
    Current: char [,]
}

let startScreenState = let empty = Array2D.create width height ' ' in {Old = empty; Current = empty}

let newScreenState screenState changeList = 
    let meaningChanges = Seq.distinctBy (fun z -> z.Position) changeList 
    let newScreen = screenState.Current.Clone() :?> char[,]
    meaningChanges 
    |> Seq.iter (fun item -> 
        newScreen.[item.Position.X, item.Position.Y] <- item.Char
        Console.SetCursorPosition(item.Position.X, item.Position.Y))
    {Old = screenState.Current; Current = newScreen}


let refreshScreen screenState = 
    let changes = seq {
        for x in 0..width - 1 do
            for y in 0..height - 1 do
                if screenState.Old.[x, y] <> screenState.Current.[x, y] then yield {Position = point x y; Char = screenState.Current.[x, y]}
    }
    for item in changes do
        if screenState.Old.[item.Position.X, item.Position.Y] <> item.Char then
            Console.SetCursorPosition(item.Position.X, item.Position.Y)
            Console.Write(item.Char)

let putChar position char changes = 
    {Position = position; Char = char} :: changes 

let startChangesList: LocatedChar list = []

(*
[<EntryPoint>]
let main args =    
    let changes = 
        startChangesList
        |> putChar (point 1 1) 'X' 
        |> putChar (point 2 1) 'Y'
    let state = newScreenState startScreenState changes
    refreshScreen state
    Console.ReadLine() |> ignore
    0
*)