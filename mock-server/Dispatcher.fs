namespace View

open System.Collections.Generic
open System
open Microsoft.FSharp.Metadata

module Dispatcher =
    
    let private instantiate name (args : obj[])=
        let asm = System.Reflection.Assembly.GetCallingAssembly()
        let fasm = FSharpAssembly.FromAssembly (asm)
        let t = fasm.GetEntity(name)
        Activator.CreateInstance(t.ReflectionType, args)

    type Dispatcher (client, server) as this =
        [<DefaultValue>] val mutable ScreenManager : IScreenManager
        [<DefaultValue>] val mutable Server : IServer
        [<DefaultValue>] val mutable Client : IClient
            
        do
            this.Client <- client
            this.Server <- server
            this.ScreenManager <- instantiate "View.ScreenManager" [| this.Client; this.Server |] :?> IScreenManager
       
    let mutable private _instance : option<Dispatcher> = None
    let instance () = _instance.Value
    let initialize client server = 
        do _instance <- Some <| Dispatcher(client, server)