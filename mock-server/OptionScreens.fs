namespace View
open Microsoft.Xna.Framework
open Ruminate.GUI.Framework
open Ruminate.GUI.Content

type MainMenu(client : IClient, server : IServer, screenManager : IScreenManager) = 
    inherit Screen()

    override this.CreateChildren () =
        this.Gui.Widgets <- [| 
            Button(10, 30, "New Game", 2, this.NewGame)
            Button(10, 60, "Options", 2, this.ShowOptions)
            Button(10, 90, "Exit", 2, this.Exit)
        |]

    member private this.Exit _ =
        client.Exit()

    member private this.ShowOptions _ =
        screenManager.Switch ScreenManagerState.OptionsMenu

    member private this.NewGame _ =
        screenManager.Switch ScreenManagerState.BoardScreen

and OptionsMenu(back) =
    inherit Screen()

    override this.CreateChildren () =
        this.Gui.Widgets <- [| 
            Button(10, 30, "Option 1", 2, (fun (x : Widget) -> (x :?> Button).Label <- "oo 1"))
            Button(10, 60, "Option 2", 2, (fun (x : Widget) -> (x :?> Button).Label <- "oo 2"))
            Button(10, 90, "Option 3", 2, (fun (x : Widget) -> (x :?> Button).Label <- "oo 3"))
            Button(10, 120, "Back", 2, back)
        |]

