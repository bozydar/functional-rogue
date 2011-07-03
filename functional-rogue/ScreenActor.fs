module ScreenActor

open System
open System.Drawing
open Board

type ScreenWritterMessage = {
    Board: Board;
    BoardFramePosition: Point
    Statistics: string
}

type screen = char[,]

let boardFrame = point 60 24
let screenSize = point 79 24

let screenWritter () =    
    let writeBoard (board: Board) (boardFramePosition: Point) (oldScreen: screen) = 
        let char item =             
            match item.Character with
            | Some(character1) -> 
                match character1.Type with
                | Avatar -> "@"
                | Monster -> "s"
                | NPC -> "P"
            | _ -> 
                match item.Items with
                | h::_ -> "i"
                | _ -> 
                    match item.Tile with
                    | Wall ->  "#"
                    | Floor -> "."
                    | _ -> " "

        let screen = Array2D.copy oldScreen
        // fill screen with board items        
        for x in 0..boardFrame.X - 1 do
            for y in 0..boardFrame.Y - 1 do
                // move board                                
                let virtualX = x + boardFramePosition.X
                let virtualY = y + boardFramePosition.Y
                screen.[x, y] <- (char board.[virtualX, virtualY]).[0]
        screen         

    let refreshScreen (oldScreen: screen) (newScreen: screen)= 
        let changes = seq {
            for x in 0..screenSize.X - 1 do
                for y in 0..screenSize.Y - 1 do
                    if oldScreen.[x, y] <> newScreen.[x, y] then yield (point x y, newScreen.[x, y])
        }
        changes
        |> Seq.iter (fun (position, char) -> 
            Console.SetCursorPosition(position.X, position.Y)
            Console.Write(char)
        )

    MailboxProcessor<ScreenWritterMessage>.Start(fun inbox ->
        let rec loop screen = async {
            let! msg = inbox.Receive()
            let newScreen =                 
                screen
                |> writeBoard msg.Board msg.BoardFramePosition
            refreshScreen screen newScreen
            return! loop newScreen
        }
        loop <| Array2D.create screenSize.X screenSize.Y ' '
    )

let agent = screenWritter ()
let refreshScreen message = agent.Post message
