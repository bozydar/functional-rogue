namespace View

open View
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Xna.Gui
open Xna.Gui.Controls
open Xna.Gui.Controls.Elements

type BoardScreen(client : IClient, server : IServer, back) = 
    inherit Screen()

    override this.CreateChildren () =
        let chars = ['.'..'Z'] |> List.map (fun item -> item.ToString())
        let charLength = List.length chars
        [| for x in 0..10 do for y in 0..10 do yield new Label( x * 20, y * 20, chars.[(x + y) % charLength]) :> Widget |]
    
    override this.OnKeyDown _ e =
        match e.KeyCode with 
        | Input.Keys.Escape -> back ()
        | _ -> ()
        