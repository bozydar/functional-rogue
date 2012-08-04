namespace FunctionalRogue.View

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Xna.Gui
open Xna.Gui.Controls
open Xna.Gui.Controls.Elements.BoardItems
open Xna.Gui.Controls.Elements

type BoardScreen(client : IClient, server : IServer, back) = 
    inherit Screen()

    override this.CreateChildren () =
        let chars = ['.'..'Z'] |> List.map (fun item -> item.ToString())
        let charLength = List.length chars
        let board = new Board()
        for x in 0..29 do
            for y in 0..29 do
                let letter = [| chars.[(x + y) % charLength].ToString() |]
                board.PutTile(x, y, new Tile(BitmapNames = letter))
        [| board :> Widget |]
    
    override this.OnKeyDown _ e =
        match e.KeyCode with 
        | Input.Keys.Escape -> back ()
        | _ -> ()
        