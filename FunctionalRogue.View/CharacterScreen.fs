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
open FunctionalRogue.State

type CharacterScreen(client : IClient, server : IServer, screenManager : IScreenManager, back) = 
    inherit Screen()

    let mutable nameIndicator = new Xna.Gui.Controls.Elements.Label(10, 10)
    let mutable hpIndicator = new Xna.Gui.Controls.Elements.Label(10, 30)
    let mutable dxIndicator = new Xna.Gui.Controls.Elements.Label(10, 50)
    let mutable stIndicator = new Xna.Gui.Controls.Elements.Label(10, 70)
    let mutable hungerLevelIndicator = new Xna.Gui.Controls.Elements.Label(10, 90)
    let mutable ironIndicator = new Xna.Gui.Controls.Elements.Label(10, 110)
    let mutable goldIndicator = new Xna.Gui.Controls.Elements.Label(10, 130)
    let mutable uraniumIndicator = new Xna.Gui.Controls.Elements.Label(10, 150)
    let mutable waterIndicator = new Xna.Gui.Controls.Elements.Label(10, 170)

    override this.OnShow () =
        let player = this.State.Player
        nameIndicator.Value <- String.Format("Name: {0}", player.Name)
        hpIndicator.Value <- String.Format("HP: {0} / {1}", player.CurrentHP, player.MaxHP)
        dxIndicator.Value <- String.Format("DX: {0}", player.Dexterity)
        stIndicator.Value <- String.Format("ST: {0}", player.Strength)
        hungerLevelIndicator.Value <- String.Format("Hunger Level: {0}", Screen.hungerLevelDescription player)
        ironIndicator.Value <- String.Format("Iron: {0}", player.Iron)
        goldIndicator.Value <- String.Format("Gold: {0}", player.Gold)
        uraniumIndicator.Value <- String.Format("Uranium: {0}", player.Uranium)
        waterIndicator.Value <- String.Format("Water: {0}", player.Water)

        this.Gui.Widgets <- [| nameIndicator; hpIndicator; dxIndicator; stIndicator; hungerLevelIndicator; 
            ironIndicator; goldIndicator; uraniumIndicator; waterIndicator |]

    override this.OnKeyDown _ e =
        match e.KeyCode with 
        | Input.Keys.Escape -> back ()
        | _ -> ()