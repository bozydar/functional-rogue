﻿namespace FunctionalRogue.View

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
        let state = State.get ()
        state.Player.Items 
        |> this.BuildEquipmentItems
        |> Seq.toArray
        

    override this.OnKeyDown _ e =
        match e.KeyCode with 
        | Input.Keys.Escape -> back ()
        | _ -> ()

    member private this.BuildEquipmentItems(equipment) =
        let itemWidget (item : Characters.Item) = 
            fun x y -> new Button(x, y, item.Name) :> Widget
        let emptyLabel = fun x y -> new Label(x, y, "   Empty   ") :> Widget

        let wearable =
            equipment 
            |> List.filter(fun item -> item.IsWearable)
        let eatable =
            equipment 
            |> List.filter(fun item -> item.IsEatable)
        let rest = 
            equipment
            |> List.filter(fun item -> not (List.exists((=) item) (wearable @ eatable)))

        let constructors = seq {
            yield fun x y -> new Label(x, y, "Wearable") :> Widget
            for item in wearable do 
                yield itemWidget item
            if List.length wearable < 1 then yield emptyLabel

            yield fun x y -> new Label(x, y, "Eatable") :> Widget
            for item in eatable do 
                yield itemWidget item
            if List.length eatable < 1 then yield emptyLabel

            yield fun x y -> new Label(x, y, "Rest") :> Widget
            for item in rest do
                yield itemWidget item
            if List.length rest < 1 then yield emptyLabel

        }
        constructors
        |> Seq.mapi (fun i item -> item 30 (30 * i))
