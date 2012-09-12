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
            let state = State.get ()
            let player = state.Player
            yield labelBuilder "TODO description here"
            if item.IsEatable then yield buttonBuilder "Eat" (fun _ -> this.OnEat state item)

            if item.IsWearable then
                if not (player.WornItems.IsWorn item) then 
                    yield buttonBuilder "Wear" (fun _ -> this.OnWear state item)
                else
                    yield buttonBuilder "Take off" (fun _ -> this.OnTakeOff state item)
            
        } |> Seq.mapi (fun i item -> item 30 (30 * i))

    override this.OnKeyDown _ e =
        match e.KeyCode with 
        | Input.Keys.Escape -> back ()
        | _ -> ()

    member this.OnEat state item =
        state.Player.Eat(item)
        let indexToRemove = List.findIndex ((=) item) state.Player.Items
        state.Player.Items <- List.removeAt indexToRemove state.Player.Items
        back ()

    member this.OnWear state item = 
        state.Player.Wear(item)
        back ()

    member this.OnTakeOff state item =
        state.Player.TakeOff (item)
        back ()
    