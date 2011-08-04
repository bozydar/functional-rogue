module Config

open System
open System.Configuration

type Settings = {
    EntireLevelSeen: bool
}

let Settings = {
    EntireLevelSeen = Convert.ToBoolean(ConfigurationManager.AppSettings.["EntireLevelSeen"])
}