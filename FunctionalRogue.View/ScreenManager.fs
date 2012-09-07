namespace FunctionalRogue.View

type ScreenManager (client, server) as this =
    let mainMenu = new MainMenu (client, server, this)
    let optionsMenu = new OptionsMenu (fun _ -> (this :> IScreenManager).Switch (ScreenManagerState.MainMenu))
    let boardScreen = new BoardScreen(client, server, this, fun _ -> (this :> IScreenManager).Switch (ScreenManagerState.MainMenu)) 
    let equipmentScreen = new EquipmentScreen(client, server, fun _ -> (this :> IScreenManager).Switch (ScreenManagerState.BoardScreen))
    
    interface IScreenManager with
        member this.Switch item = 
            match item with
            | ScreenManagerState.MainMenu -> client.Show(mainMenu)
            | ScreenManagerState.OptionsMenu -> client.Show(optionsMenu)
            | ScreenManagerState.BoardScreen -> client.Show(boardScreen)
            // equipmentScreen can be instatiated here if there would be a need to transprot some data
            // through "back" function 
            | ScreenManagerState.EquipmentScreen -> client.Show(equipmentScreen)