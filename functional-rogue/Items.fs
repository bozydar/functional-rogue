module Items

open System

type Item = {    
    Id : Guid;
    Name : string;
    Wearing : Wearing
    Offence : Factor;
    Defence : Factor;
    Type : Type;
    MiscProperties : MiscProperties;
} 
and Wearing = {
    OnHead : bool;
    InHand : bool;
    OnTorso : bool;
    OnLegs : bool;
} 
with
    static member NotWearable = { OnHead = false; InHand = false; OnTorso = false; OnLegs = false }
    static member HeadOnly = { Wearing.NotWearable with OnHead = true }
    static member HandOnly = { Wearing.NotWearable with InHand = true }
    static member TorsoOnly = { Wearing.NotWearable with OnTorso = true }
    static member LegsOnly = { Wearing.NotWearable with OnLegs = true }
and Type = 
    | Stick
    | Rock
    | Sword
    | Hat
    | Corpse
    | Tool
and MiscProperties = {
    OreExtractionRate : int
}

let defaultMiscProperties = {
    OreExtractionRate = 0
}

let itemShortDescription item =
    let rest = 
        item.Name + " - "
        + if not item.Offence.IsZero then sprintf "Offence: %s " (item.Offence.ToString()) else ""  
        + if not item.Defence.IsZero  then sprintf "Defence: %s " (item.Defence.ToString()) else ""
    rest
