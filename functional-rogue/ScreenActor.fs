module ScreenActor

open System
open System.Drawing
open Board

type ScreenWritterMessage = {
    Board: Board;
    BoardFramePosition: Point
    Statistics: Statistics
} and Statistics = {
    HP: int;  // life
    MaxHP: int;
    Magic: int;  // magic
    MaxMagic: int;
    Gold: int // gold
}

type screen = char[,]

let boardFrame = point 60 24
let screenSize = point 79 24
let leftPanelPos = new Rectangle(61, 0, 19, 24)

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
        
    let writeString (position: Point) text (screen: screen) = 
        let x = position.X
        let y = position.Y
        text
        |> String.iteri (fun i char -> 
            screen.[x + i, y] <- char)
        screen

    let writeStats stat screen =
        let loc = point leftPanelPos.Location.X (leftPanelPos.Location.Y + 1)
        screen 
        |> writeString leftPanelPos.Location (sprintf "HP: %d/%d" stat.HP stat.MaxHP)
        |> writeString loc (sprintf "Ma: %d/%d" stat.HP stat.MaxHP)
        

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
                |> writeStats msg.Statistics
            refreshScreen screen newScreen
            return! loop newScreen
        }
        loop <| Array2D.create screenSize.X screenSize.Y ' '
    )

let agent = screenWritter ()
let refreshScreen message = agent.Post message
