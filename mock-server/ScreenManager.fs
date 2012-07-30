namespace View

type ScreenManager (client, server) as this =
    let mainMenu = new MainMenu (client, server, this)
    let optionsMenu = new OptionsMenu (fun _ -> (this :> IScreenManager).Switch (ScreenManagerState.MainMenu))
    let boardScreen = new BoardScreen(client, server, fun _ -> (this :> IScreenManager).Switch (ScreenManagerState.MainMenu)) 
    
    interface IScreenManager with
        member this.Switch item = 
            match item with
            | ScreenManagerState.MainMenu -> client.Show(mainMenu)
            | ScreenManagerState.OptionsMenu -> client.Show(optionsMenu)
            | ScreenManagerState.BoardScreen -> client.Show(boardScreen)
            