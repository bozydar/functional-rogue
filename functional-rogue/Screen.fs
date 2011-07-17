module Screen

open System
open System.Drawing
open Board
open State

type ScreenAgentMessage =
    | ShowBoard of State


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

        let screen: screen = Array2D.copy oldScreen
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

    let writeStats state screen =
        screen 
        |> writeString leftPanelPos.Location (sprintf "HP: %d/%d" state.Player.HP state.Player.MaxHP)
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 1)) (sprintf "Ma: %d/%d" state.Player.HP state.Player.MaxHP)
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 2)) (sprintf "Turn: %d" state.TurnNumber)
        

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

    MailboxProcessor<ScreenAgentMessage>.Start(fun inbox ->
        let rec loop screen = async {
            let! msg = inbox.Receive()

            let newScreen = match msg with
                            | ShowBoard(state) ->                              
                                screen
                                |> writeBoard state.Board state.BoardFramePosition
                                |> writeStats state
            refreshScreen screen newScreen                
            return! loop newScreen                        
        }
        loop <| Array2D.create screenSize.X screenSize.Y empty
    )

let private agent = screenWritter ()
let showBoard () = agent.Post (ShowBoard(State.get ()))
