namespace Server
open Screens

type Server(client : IClient) as this =
    do Dispatcher.instance.Server <- this
    interface IServer

module Instance =
    
    [<CompiledNameAttribute("Connect")>]
    let connect (client : IClient) = 

//        let mainMenu = 
//            let result = new MainMenu.MainMenu()
//            result.OptionSelected <- (function  
//                | MainMenu.Exit -> client.Exit ()
//                | MainMenu.Options -> client.Show())
//            result

        let server = Server(client)
        Dispatcher.instance.ScreenManager.Switch(ScreenManagerState.MainMenu)
