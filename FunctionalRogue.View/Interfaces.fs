namespace FunctionalRogue.View

open System
open System.Collections.Generic
open Microsoft.Xna.Framework
open Xna.Gui
open Xna.Gui.Rendering
open Xna.Gui.Utils
open Xna.Gui.Controls

[<AbstractClassAttribute>]
type Screen () =
    [<DefaultValue>] val mutable Color : Color
    [<DefaultValue>] val mutable Gui : Gui
    [<DefaultValue>] val mutable IsInitialized : bool

    abstract member Init : Game -> Skin -> TextRenderer -> unit 
    default this.Init game skin textRenderer =
        if not this.IsInitialized then
            this.Gui <- Gui(game, skin, textRenderer)    
            this.IsInitialized <- true
            this.OnInit ()
        
        this.Gui.BindInput ()
        this.OnShow ()
        this.Gui.KeyDown.AddHandler(fun sender e -> this.OnKeyDown sender e)

    abstract member OnInit : unit -> unit
    default this.OnInit () = ()

    abstract member OnShow : unit -> unit
    default this.OnShow () = () 

    abstract member OnKeyDown : obj -> KeyEventArgs -> unit
    default this.OnKeyDown _ e = ()
    
    abstract member Unload : unit -> unit
    default this.Unload () =
        this.Gui <- null

    abstract member Update : unit -> unit
    default this.Update () =
        this.Gui.Update()

    abstract member Draw : unit -> unit
    default this.Draw() =
        this.Gui.Draw()

    member this.RegisterKeyEvent key event =
        this.Gui.KeyDown.AddHandler(
            fun sender keyEventArgs -> 
                match keyEventArgs.KeyCode with 
                | keyCode when keyCode = key -> event sender
                | _ -> ())

and public IClient = 
    abstract member Show : Screen -> unit
    abstract member Exit : unit -> unit

and public IServer = interface end


type IScreenManager = 
    abstract member Switch : ScreenManagerState -> unit

type IDispatcher =
    abstract member Register : obj -> unit