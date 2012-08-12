namespace FunctionalRogue.View

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open System.Drawing
open System
open Xna.Gui
open Xna.Gui.Controls
open Xna.Gui.Controls.Elements.BoardItems
open Xna.Gui.Controls.Elements
open FunctionalRogue
open FunctionalRogue.Board
open FunctionalRogue.Screen

type BoardScreen(client : IClient, server : IServer, back) = 
    inherit Screen()

    let boardFrameSize = new Size(60, 23)
    [<DefaultValue>]
    val mutable boardWidget : Xna.Gui.Controls.Elements.Board

    let toTextel item (highlighOption : ConsoleColor option) =  
        if item.WasSeen then
            let result = 
                let character = 
                    if item.IsSeen && item.Character.IsSome then
                        match item.Character.Value.Type with
                        | Avatar -> {Char = '@'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                        | Monster -> {Char = item.Character.Value.Appearance; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Red}
                        | NPC -> {Char = 'P'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.White}
                        else
                        FunctionalRogue.Screen.empty
                if character <> empty then
                    character
                else
                    match item.Items with
                    | h::_ when not <| Set.contains item.Tile obstacles -> 
                            itemToTextel h
                    | _ -> 
                        match item.Ore with
                        | Iron(_) -> {Char = '$'; FGColor = ConsoleColor.Black; BGColor = ConsoleColor.Gray}
                        | Gold(_) -> {Char = '$'; FGColor = ConsoleColor.Black; BGColor = ConsoleColor.Yellow}
                        | Uranium(_) -> {Char = '$'; FGColor = ConsoleColor.Black; BGColor = ConsoleColor.Green}
                        | CleanWater(_) -> {Char = '~'; FGColor = ConsoleColor.Blue; BGColor = ConsoleColor.Black}
                        | ContaminatedWater(_) -> {Char = '~'; FGColor = ConsoleColor.Blue; BGColor = ConsoleColor.Black}
                        | _ ->
                            let mainMapBackground = if highlighOption.IsSome then highlighOption.Value else ConsoleColor.Black
                            match item.Tile with
                            | Wall ->  {Char = '#'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                            | Floor -> {Char = '.'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                            | OpenDoor -> {Char = '/'; FGColor = ConsoleColor.DarkGray; BGColor = ConsoleColor.Black}
                            | ClosedDoor -> {Char = '+'; FGColor = ConsoleColor.DarkGray; BGColor = ConsoleColor.Black}
                            | Grass -> {Char = '.'; FGColor = ConsoleColor.DarkGreen; BGColor = ConsoleColor.Black}
                            | Tree -> {Char = 'T'; FGColor = ConsoleColor.DarkGreen; BGColor = ConsoleColor.Black}
                            | SmallPlants -> {Char = '*'; FGColor = ConsoleColor.DarkGreen; BGColor = ConsoleColor.Black}
                            | Bush -> {Char = '&'; FGColor = ConsoleColor.DarkGreen; BGColor = ConsoleColor.Black}
                            | Glass -> {Char = '#'; FGColor = ConsoleColor.Blue; BGColor = ConsoleColor.Black}
                            | Sand -> {Char = '.'; FGColor = ConsoleColor.Yellow; BGColor = ConsoleColor.Black}
                            | Tile.Water -> {Char = '~'; FGColor = ConsoleColor.Blue; BGColor = ConsoleColor.Black}
                            | StairsDown -> {Char = '>'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                            | StairsUp -> {Char = '<'; FGColor = ConsoleColor.White; BGColor = ConsoleColor.Black}
                            | Computer -> {Char = '#'; FGColor = ConsoleColor.Red; BGColor = ConsoleColor.Black}
                            | Replicator -> {Char = '_'; FGColor = ConsoleColor.Red; BGColor = ConsoleColor.Black}
                            | MainMapForest -> {Char = '&'; FGColor = ConsoleColor.DarkGreen; BGColor = mainMapBackground}
                            | MainMapGrassland -> {Char = '"'; FGColor = ConsoleColor.Green; BGColor = mainMapBackground}
                            | MainMapMountains -> {Char = '^'; FGColor = ConsoleColor.Gray; BGColor = mainMapBackground}
                            | MainMapWater -> {Char = '~'; FGColor = ConsoleColor.Blue; BGColor = mainMapBackground}
                            | MainMapCoast -> {Char = '.'; FGColor = ConsoleColor.Yellow; BGColor = mainMapBackground}
                            | _ -> empty
            if not item.IsSeen then {result with FGColor = ConsoleColor.DarkGray }
            else
                if item.Features |> List.exists (fun feature -> match feature with | OnFire(value) -> true | _ -> false) then
                    let randomResult = rnd 10
                    match randomResult with
                    | value when value < 1 -> {Char = '&'; FGColor = ConsoleColor.Red; BGColor = ConsoleColor.Black}
                    | value when value < 2 -> {Char = '&'; FGColor = ConsoleColor.Yellow; BGColor = ConsoleColor.Black}
                    | _ -> result
                else
                    result
        else empty

    override this.CreateChildren () =
        let chars = ['.'..'Z'] |> List.map (fun item -> item.ToString())
        let charLength = List.length chars
        this.boardWidget <- new Xna.Gui.Controls.Elements.Board(boardFrameSize.Width, boardFrameSize.Height)
        for x in 0..boardFrameSize.Width do
            for y in 0..boardFrameSize.Height do
                let letter = [| chars.[(x + y) % charLength].ToString() |]
                this.boardWidget.PutTile(x, y, new Xna.Gui.Controls.Elements.BoardItems.Tile(BitmapNames = letter))
        [| this.boardWidget :> Widget |]

    member this.ShowBoard (board: Board, boardFramePosition: Point) =
        for x in 0..boardFrameSize.Width - 1 do
            for y in 0..boardFrameSize.Height - 1 do
                // move board                                
                let virtualX = x + boardFramePosition.X
                let virtualY = y + boardFramePosition.Y
                this.boardWidget.PutTile(x, y, 
                    new Xna.Gui.Controls.Elements.BoardItems.Tile(
                        BitmapNames = 
                            [| 
                                board.Places.[virtualX, virtualY].Character.Value.ToString() 
                            |]))
                //screen.[x, y] <- toTextel board.Places.[virtualX, virtualY] (getHighlightForTile board virtualX virtualY)
    
    override this.OnKeyDown _ e =
        match e.KeyCode with 
        | Input.Keys.Escape -> back ()
        | _ -> ()
        