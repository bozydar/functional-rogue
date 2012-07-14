namespace Server
open System.Collections.Generic
open System
module Dispatcher =
    
    type Dispatcher () =
        [<DefaultValue>] val mutable ScreenManager : IScreenManager
        [<DefaultValue>] val mutable Server : IServer
        [<DefaultValue>] val mutable Client : IClient
        
    let instance = new Dispatcher ()        