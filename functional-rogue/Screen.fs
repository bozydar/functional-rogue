﻿module Screen

open System
open System.Drawing
open Board
open State
open Sight

type ScreenAgentMessage =
    | ShowBoard of State
    | ShowMainMenu of AsyncReplyChannel<MainMenuReply>
and MainMenuReply = {
    Name: String
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
    let writeBoard (board: Board) (boardFramePosition: Point) sightRadius (screen: screen)  = 
        let toTextel item =  
            if item.WasSeen then
                let result = 
                    match item.Character with
                    | Some(character1) -> 
                        match character1.Type with
                        | Avatar -> {Char = '@'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                        | Monster -> {Char = 's'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Red}
                        | NPC -> {Char = 'P'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.White}
                    | _ -> 
                        match item.Items with
                        | h::_ -> 
                                match h with 
                                | Gold(value) -> {Char = '$'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                                | _ -> {Char = 'i'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                        | _ -> 
                            match item.Tile with
                            | Wall ->  {Char = '#'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                            | Floor -> {Char = '.'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                            | _ -> empty
                if not item.IsSeen then {result with FGColor = ConsoleColor.DarkGray } else result
            else empty
        for x in 0..boardFrame.X - 1 do
            for y in 0..boardFrame.Y - 1 do
                // move board                                
                let virtualX = x + boardFramePosition.X
                let virtualY = y + boardFramePosition.Y
                screen.[x, y] <- toTextel board.[virtualX, virtualY]
        screen      
        
    let writeString (position: Point) (text: String) (screen: screen) = 
        let x = position.X
        let y = position.Y
        let length = min (screenSize.X - x) text.Length
        text.Substring(0, length)
        |> String.iteri (fun i char -> 
            screen.[x + i, y] <- {empty with Char = char})
        screen

    let cleanScreen screen =
        for x in 0..Array2D.base1 screen do
            for y in 0..Array2D.base2 screen do
                screen.[x, y] <- empty
        screen

    let writeStats state screen =
        screen 
        |> writeString leftPanelPos.Location (sprintf "HP: %d/%d" state.Player.HP state.Player.MaxHP)
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 1)) (sprintf "%s the rogue" state.Player.Name)
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 2)) (sprintf "Ma: %d/%d" state.Player.HP state.Player.MaxHP)
        |> writeString (point leftPanelPos.Location.X (leftPanelPos.Location.Y + 3)) (sprintf "Turn: %d" state.TurnNumber)
        

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

            match msg with
            | ShowBoard(state) -> let newScreen = 
                                      screen
                                      |> Array2D.copy
                                      |> writeBoard state.Board state.BoardFramePosition state.Player.SightRadius
                                      |> writeStats state
                                  refreshScreen screen newScreen
                                  return! loop newScreen                        
            | ShowMainMenu(reply) -> let newScreen = 
                                         screen
                                         |> Array2D.copy
                                         |> cleanScreen
                                         |> writeString (point 1 1) "What is your name?"
                                     refreshScreen screen newScreen
                                     Console.SetCursorPosition(1, 2)
                                     let name = Console.ReadLine()
                                     reply.Reply({Name = name})
                                     return! loop newScreen  
        }
        loop <| Array2D.create screenSize.X screenSize.Y empty
    )

let private agent = screenWritter ()
let showBoard () = agent.Post (ShowBoard(State.get ()))
let showMainMenu () = agent.PostAndReply(fun reply -> ShowMainMenu(reply))
