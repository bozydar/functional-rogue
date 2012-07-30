module ScreenManager 
    open Screens
    open Server
    open Dispatcher

    type ScreenManager (client, server) as this =
        let mainMenu = new MainMenu (client, server, this)
        // TODO: Change to work it by ScreenManager
        let optionsMenu = new OptionsMenu (fun _ -> (this :> IScreenManager).Switch (ScreenManagerState.MainMenu))
        let boardScreen = new BoardScreen(client, server) 
    
        interface IScreenManager with
            member this.Switch item = match item with
                | ScreenManagerState.MainMenu -> client.Show(mainMenu)
                | ScreenManagerState.OptionsMenu -> client.Show(optionsMenu)
                | ScreenManagerState.BoardScreen -> client.Show(boardScreen)
            