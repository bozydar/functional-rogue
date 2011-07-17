module Screen

open System
open System.Drawing
open Board

type ScreenMessage = {
    BoardFramePosition: Point
    Statistics: Statistics
} and Statistics = {
    HP: int;  // life
    MaxHP: int;
    Magic: int;  // magic
    MaxMagic: int;
    Gold: int // gold
}

type textel = {
    Char: char;
    BGColor: ConsoleColor;
    FGColor: ConsoleColor
} 
    
let private empty = {Char = ' '; FGColor = ConsoleColor.Gray; BGColor = ConsoleColor.Black}

type private screen = textel[,]

let boardFrame = point 60 24
let private screenSize = point 79 24
let private leftPanelPos = new Rectangle(61, 0, 19, 24)

let private screenWritter () =    
    let writeBoard (board: Board) (boardFramePosition: Point) (oldScreen: screen) = 
        let toTextel item =             
            match item.Character with
            | Some(character1) -> 
                match character1.Type with
                | Avatar -> {Char = '@'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                | Monster -> {Char = 's'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Red}
                | NPC -> {Char = 'P'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.White}
            | _ -> 
                match item.Items with
                | h::_ -> {Char = 'i'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                | _ -> 
                    match item.Tile with
                    | Wall ->  {Char = '#'; FGColor = ConsoleColor.Gray; BGColor = ConsoleColor.Black}
                    | Floor -> {Char = '.'; FGColor = ConsoleColor.DarkGray; BGColor = ConsoleColor.Black}
                    | _ -> empty

        let screen = Array2D.copy oldScreen
        // fill screen with board items        
        for x in 0..boardFrame.X - 1 do
            for y in 0..boardFrame.Y - 1 do
                // move board                                
                let virtualX = x + boardFramePosition.X
                let virtualY = y + boardFramePosition.Y
                screen.[x, y] <- toTextel board.[virtualX, virtualY]
        screen      
        
    let writeString (position: Point) text (screen: screen) = 
        let x = position.X
        let y = position.Y
        text
        |> String.iteri (fun i char -> 
            screen.[x + i, y] <- {empty with Char = char})
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
        |> Seq.iter (fun (position, textel) -> 
            Console.SetCursorPosition(position.X, position.Y)
            Console.BackgroundColor <- textel.BGColor
            Console.ForegroundColor <- textel.FGColor
            Console.Write(textel.Char)
        )
        Console.SetCursorPosition(screenSize.X, screenSize.Y)

    MailboxProcessor<ScreenMessage>.Start(fun inbox ->
        let rec loop screen = async {
            let! msg = inbox.Receive()
            let state = State.get ()
            let newScreen =                 
                screen
                |> writeBoard state.Board msg.BoardFramePosition
                |> writeStats msg.Statistics
            refreshScreen screen newScreen
            return! loop newScreen
        }
        loop <| Array2D.create screenSize.X screenSize.Y empty
    )

let private agent = screenWritter ()
let refresh message = agent.Post message
