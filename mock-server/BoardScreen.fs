namespace View

open View
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Ruminate.GUI.Framework
open Ruminate.GUI.Content

type BoardScreen(client : IClient, server : IServer) = 
    inherit Screen()

    override this.CreateChildren () =
        let chars = ['.'..'Z'] |> List.map (fun item -> item.ToString())
        let charLength = List.length chars
        this.Gui.Widgets <- [| for x in 0..10 do for y in 0..10 do yield new Label( x * 20, y * 20, chars.[(x + y) % charLength]) :> Widget |]
        