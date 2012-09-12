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

type BoardScreen(client : IClient, server : IServer, screenManager : IScreenManager, back) = 
    inherit Screen()

    let boardFrameSize = new Size(60, 23)
    [<DefaultValue>]
    val mutable boardWidget : Xna.Gui.Controls.Elements.Board

    override this.OnInit () =
        Screen.agent.Agent <- this.MailboxProcessor ()
        Game.subscribeHandlers ()
        Game.initialize ()

    override this.OnShow () =
        this.boardWidget <- new Xna.Gui.Controls.Elements.Board(boardFrameSize.Width, boardFrameSize.Height)
        Game.makeAction (System.ConsoleKeyInfo ('5', ConsoleKey.NumPad5, false, false, false))
        this.Gui.Widgets <- [| this.boardWidget :> Widget |]

    member this.ShowBoard (board: Board, boardFramePosition: Point) =
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
                this.boardWidget.PutTile(x, y, tile)
    
    [<DefaultValue>] val mutable keyBuffer : System.ConsoleKeyInfo

    override this.OnKeyDown _ e =
        match e.KeyCode with 
        | Input.Keys.Escape -> back ()
        | Input.Keys.I -> screenManager.Switch(ScreenManagerState.EquipmentScreen)
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
                        this.ShowBoard (state.Board, state.BoardFramePosition)
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
        