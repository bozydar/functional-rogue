namespace FunctionalRogue.View

open Xna.Gui
open Xna.Gui.Controls.Elements

[<AutoOpen>]
module ScreenUtils =
    let buttonBuilder name (event : Widget -> unit) = fun x y -> new Button(x, y, name, 2, event) :> Widget
    let labelBuilder text = fun x y -> new Label(x, y, text) :> Widget
