namespace Server
open Screens

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
        client.Show(new MainMenu(client, server))
