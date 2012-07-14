namespace Server

open System
open System.Collections.Generic
open Microsoft.Xna.Framework
open Ruminate.GUI.Framework
open Ruminate.GUI.Content


[<AbstractClassAttribute>]
type Screen () =
    [<DefaultValue>] val mutable Color : Color
    [<DefaultValue>] val mutable Gui : Gui
    [<DefaultValue>] val mutable IsInitialized : bool

    abstract member Init : Game -> Skin -> TextRenderer -> unit 
    default this.Init game skin textRenderer =
        if not this.IsInitialized then
            this.Gui <- Gui(game, skin, textRenderer)        
            this.CreateChildren ()
            this.IsInitialized <- true

    abstract member CreateChildren : unit -> unit
    abstract member Unload : unit -> unit
    default this.Unload () =
        this.Gui <- null
    abstract member Update : unit -> unit
    default this.Update () =
        this.Gui.Update()
    abstract member Draw : unit -> unit
    default this.Draw() =
        this.Gui.Draw()

and public IClient = 
    abstract member Show : Screen -> unit
    abstract member Exit : unit -> unit

and public IServer = interface end

type Server(client : IClient) =
    interface IServer
