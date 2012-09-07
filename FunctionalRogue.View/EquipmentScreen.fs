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

type EquipmentScreen(client : IClient, server : IServer, back) = 
    inherit Screen()

    override this.CreateChildren () =
        // TODO: Create list of equipment items. Shown items should be filtered by some function
        // There should be two modes:
        // 1 - selecting item and executing some action:
        //     - use
        //     - drop
        //     - wear
        // 2 - select n items and return what items were selected
        [|  Button(10, 30, "Eq1", 2, fun _ -> () )
            Button(10, 60, "Eq2", 2, fun _ -> () )
            Button(10, 90, "Eq3", 2, fun _ -> () ) |]

    override this.OnKeyDown _ e =
        match e.KeyCode with 
        | Input.Keys.Escape -> back ()
        | _ -> ()
