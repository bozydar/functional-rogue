﻿module Items

type Item = {    
    Id : int;
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
and Type = 
    | Stick
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

type PredefinedItems =
    | OreExtractor
    
let createPredefinedItem predefinedItem =
    match predefinedItem with
    | OreExtractor ->
        { Id = 0;
            Name = "Ore Extractor";
            Wearing = { OnHead = false;
                        InHand = true;
                        OnTorso = false;
                        OnLegs = false
            };
            Offence = Value(0M);
            Defence = Value(0M);
            Type = Tool;
            MiscProperties = { defaultMiscProperties with OreExtractionRate = 1 }
        }   