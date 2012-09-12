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

type UsageScreen(client : IClient, server : IServer, back) = 
    inherit Screen()

    [<DefaultValue>] val mutable EquipmentItem : Characters.Item

    override this.OnShow () =
        this.Gui.Widgets <- 
            this.ShowEquipment this.EquipmentItem
            |> Seq.toArray

    member this.ShowEquipment (item : Characters.Item) = 
        seq {
            yield new Label(30, 30, "TODO description here") :> Widget
            if item.IsEatable then yield new Button(30, 60, "Eat") :> Widget
            if item.IsWearable then yield new Button(30, 90, "Wear") :> Widget
        }


    