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

        client.Show(new MainMenu(client))
        Server(client)
