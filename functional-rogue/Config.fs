module Config

open System
open System.Configuration

type Settings = {
    EntireLevelSeen : bool
    LoadSave : bool
}

let Settings = {
    EntireLevelSeen = Convert.ToBoolean(ConfigurationManager.AppSettings.["EntireLevelSeen"])
    LoadSave = Convert.ToBoolean(ConfigurationManager.AppSettings.["LoadSave"])
}