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

type BoardScreen(client : IClient, server : IServer, screenManager : IScreenManager, back) = 
    inherit Screen()

    let boardFrameSize = new Size(30, 23)
    let panelPosition = new Point(600, 20) 

    let mutable boardWidget = new Xna.Gui.Controls.Elements.Board(boardFrameSize.Width, boardFrameSize.Height)
    let mutable hpIndicator  : Xna.Gui.Controls.Elements.Label = new Xna.Gui.Controls.Elements.Label(panelPosition.X, panelPosition.Y + 20)
    let mutable hungerIndicator  : Xna.Gui.Controls.Elements.Label = new Xna.Gui.Controls.Elements.Label(panelPosition.X, panelPosition.Y + 40)
    let mutable turnIndicator  : Xna.Gui.Controls.Elements.Label = new Xna.Gui.Controls.Elements.Label(panelPosition.X, panelPosition.Y + 60)
    let mutable mapLevelIndicator  : Xna.Gui.Controls.Elements.Label = new Xna.Gui.Controls.Elements.Label(panelPosition.X, panelPosition.Y + 80)

    member private this.EvaluateBoardFramePosition (state : State) = 
        let playerPosition = getPlayerPosition state.Board
        
        let x = inBoundary (playerPosition.X - (boardFrameSize.Width / 2)) 0 (boardWidth - boardFrameSize.Width) 
        let y = inBoundary (playerPosition.Y - (boardFrameSize.Height / 2)) 0 (boardHeight - boardFrameSize.Height)
        point x y                

    override this.OnInit () =
        Screen.agent.Agent <- this.MailboxProcessor ()
        Game.subscribeHandlers ()
        Game.initialize ()

    override this.OnShow () =
        Game.makeAction (System.ConsoleKeyInfo ('5', ConsoleKey.NumPad5, false, false, false))
        this.Gui.Widgets <- [| boardWidget :> Widget; hpIndicator; hungerIndicator; turnIndicator; mapLevelIndicator |]

    member private this.ShowBoard (state : State) =
        let boardFramePosition = this.EvaluateBoardFramePosition(state)
        let board = state.Board
        for x in 0..boardFrameSize.Width - 1 do
            for y in 0..boardFrameSize.Height - 1 do
                // move board                                
                let virtualX = x + boardFramePosition.X
                let virtualY = y + boardFramePosition.Y
                let textel = FunctionalRogue.Screen.toTextel board.Places.[virtualX, virtualY] None
                let tile = 
                    new Xna.Gui.Controls.Elements.BoardItems.Tile (
                        BitmapNames = 
                            [| 
                                textel.Char.ToString()
                            |])
                boardWidget.PutTile(x, y, tile)
    
    member private this.ShowStats (state : State) =
        hpIndicator.Value <- String.Format("HP: {0}/{1}", state.Player.CurrentHP, state.Player.MaxHP)
        hungerIndicator.Value <- Screen.hungerLevelDescription state.Player 
        turnIndicator.Value <- String.Format("Turn: {0}", state.TurnNumber)


    [<DefaultValue>] val mutable keyBuffer : System.ConsoleKeyInfo

    override this.OnKeyDown _ e =
        match e.KeyCode with 
        | Input.Keys.Escape -> back ()
        | Input.Keys.I -> screenManager.Switch(ScreenManagerState.EquipmentScreen)
        | Input.Keys.C -> screenManager.Switch(ScreenManagerState.CharacterScreen)
        | _ ->
            let keyInfo = new System.ConsoleKeyInfo (Convert.ToChar(0), enum (int e.KeyCode), false, false, false)
            Game.makeAction keyInfo
            // TODO: This line has to be removed when I remove all ReadKey calls
            this.keyBuffer <- keyInfo  

    member this.MailboxProcessor () : MailboxProcessor<Screen.ScreenAgentMessage> =
        // I don't know why this mailbox is not called although Key are catched and Game is processed.
        MailboxProcessor<Screen.ScreenAgentMessage>.Start(fun inbox ->
            let rec loop () = async {
                try
                    let! msg = inbox.Receive ()
                    match msg with
                    | ShowBoard(state) -> 
                        this.ShowBoard (state)
                        this.ShowStats (state)
                        return! loop ()
                    | ReadKey(reply) ->
                        reply.Reply { ConsoleKeyInfo = this.keyBuffer }
                        return! loop ()
                    | _ -> return! loop ()
                with
                    | ex -> logException Error "BoardScreen" ex
            }
            loop () 
        )
        