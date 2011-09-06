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

type PredefinedItems =
    | RandomStick
    | RandomRock
    | OreExtractor
    
let createPredefinedItem predefinedItem =
    match predefinedItem with
    | RandomStick ->
        {
        Id = System.Guid.NewGuid();
        Name = "Stick";
        Wearing = {
                    OnHead = false;
                    InHand = true;
                    OnTorso = false;
                    OnLegs = false
        };
        Offence = Value((decimal)(rnd2 1 3));
        Defence = Value(0M);
        Type = Stick;
        MiscProperties = defaultMiscProperties
        }
    | RandomRock ->
        {
        Id = System.Guid.NewGuid();
        Name = "Rock";
        Wearing = {
                    OnHead = false;
                    InHand = true;
                    OnTorso = false;
                    OnLegs = false
        };
        Offence = Value((decimal)(rnd2 1 3));
        Defence = Value(0M);
        Type = Rock;
        MiscProperties = defaultMiscProperties
        }
    | OreExtractor ->
        { Id = Guid.NewGuid();
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

let createRandomNaturalItem (playerLevel: int) =
    //later player level will be used to determine probability of some more powerful/valuable items
    let randomResult = rnd 2
    if randomResult < 1 then
        createPredefinedItem RandomRock
    else
        createPredefinedItem RandomStick