namespace FunctionalRogue.View

type Server(client : IClient) =
    interface IServer

module Instance =
    
    [<CompiledNameAttribute("Connect")>]
    let connect (client : IClient) = 
        let server = Server(client)
        Dispatcher.initialize client server |> ignore
        Dispatcher.instance().ScreenManager.Switch(ScreenManagerState.MainMenu)