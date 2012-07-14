// Learn more about F# at http://fsharp.net

namespace Screens
open Microsoft.Xna.Framework
open Ruminate.GUI.Framework
open Ruminate.GUI.Content

[<AbstractClassAttribute>]
type Screen() =
    [<DefaultValue>] val mutable Color : Color
    [<DefaultValue>] val mutable Gui : Gui

    abstract member Init : Game -> Skin -> TextRenderer -> unit
    abstract member Unload : unit -> unit
    default this.Unload () =
        this.Gui <- null
    abstract member Update : unit -> unit
    default this.Update () =
        this.Gui.Update()
    abstract member Draw : unit -> unit
    default this.Draw() =
        this.Gui.Draw()

module MainMenu =
    type MainMenuSelected =
    | NewGame
    | Options
    | Exit

    type MainMenu() = 
        inherit Screen()
        [<DefaultValue>] val mutable OptionSelected : MainMenuSelected -> unit
    
        override this.Init game skin textRenderer =
            this.Gui <- Gui(game, skin, textRenderer)
            this.Gui.Widgets <- [| 
                Button(10, 30, "New Game", 2, (fun _ -> this.OptionSelected NewGame))
                Button(10, 60, "Options", 2, (fun _ -> this.OptionSelected Options))
                Button(10, 90, "Exit", 2, (fun _ -> this.OptionSelected Exit))
            |]
        
    
module GameScreen =
    type GameScreen() =
        inherit Screen()

        override this.Init game skin textRenderer =
            this.Gui <- Gui(game, skin, textRenderer)
            this.Gui.Widgets <- [| 
                Button(10, 30, "Option 1", 2, (fun (x : Widget) -> (x :?> Button).Label <- "new game started"))
                Button(10, 60, "Option 2", 2, (fun (x : Widget) -> (x :?> Button).Label <- "new game started"))
                Button(10, 90, "Option 3", 2, (fun (x : Widget) -> (x :?> Button).Label <- "new game started"))

            |]

