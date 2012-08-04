namespace FunctionalRogue

module Config = 

    open System
    open System.Configuration

    type Settings = {
        EntireLevelSeen : bool
        LoadSave : bool
        TestStartOnEmpty : bool
    }

    let Settings = {
        EntireLevelSeen = Convert.ToBoolean(ConfigurationManager.AppSettings.["EntireLevelSeen"])
        LoadSave = Convert.ToBoolean(ConfigurationManager.AppSettings.["LoadSave"])
        TestStartOnEmpty = Convert.ToBoolean(ConfigurationManager.AppSettings.["TestStartOnEmpty"])
    }