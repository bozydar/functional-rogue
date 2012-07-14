// Learn more about F# at http://fsharp.net

namespace Screens
open Server
open Microsoft.Xna.Framework
open Ruminate.GUI.Framework
open Ruminate.GUI.Content
open Server

type MainMenu(client : IClient) = 
    inherit Screen()
    [<DefaultValue>] val mutable OptionsMenu : OptionsMenu

    override this.CreateChildren () =
        this.Gui.Widgets <- [| 
            Button(10, 30, "New Game", 2, null)
            Button(10, 60, "Tralala", 2, this.ShowOptions)
            Button(10, 90, "Exit", 2, this.Exit)
        |]
        this.OptionsMenu <- OptionsMenu(fun _ -> client.Show(this))
        
    member private this.Exit _ =
        client.Exit()

    member private this.ShowOptions _ =
        client.Show(this.OptionsMenu)

and OptionsMenu(back) =
    inherit Screen()

    override this.CreateChildren () =
        this.Gui.Widgets <- [| 
            Button(10, 30, "Option 1", 2, (fun (x : Widget) -> (x :?> Button).Label <- "oo 1"))
            Button(10, 60, "Option 2", 2, (fun (x : Widget) -> (x :?> Button).Label <- "oo 2"))
            Button(10, 90, "Option 3", 2, (fun (x : Widget) -> (x :?> Button).Label <- "oo 3"))
            Button(10, 120, "Back", 2, back)
        |]


