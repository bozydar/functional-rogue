module Items

open System
open Quantity

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
    | Knife
    | Hat
    | Corpse
    | Tool
and MiscProperties = {
    OreExtractionRate : int
}

type Ore = 
    | NoneOre
    | Iron of Quantity
    | Gold of Quantity
    | Uranium of Quantity
    | CleanWater of Quantity
    | ContaminatedWater of Quantity 
    member this.Quantity 
        with get() = 
            match this with
            | Iron(value) -> value
            | Gold(value) -> value
            | Uranium(value) -> value
            | CleanWater(value) -> value
            | ContaminatedWater(value) -> value
            | _ -> QuantityValue(0)

let defaultMiscProperties = {
    OreExtractionRate = 0
}

let defaultWearing = {
    OnHead = false; InHand = false; OnTorso = false; OnLegs = false
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
    | CombatKnife
    | OreExtractor
    
let createPredefinedItem predefinedItem =
    match predefinedItem with
    | RandomStick ->
        {
        Id = System.Guid.NewGuid();
        Name = "Stick";
        Wearing = { defaultWearing with InHand = true };
        Offence = Value((decimal)(rnd2 1 3));
        Defence = Value(0M);
        Type = Stick;
        MiscProperties = defaultMiscProperties
        }
    | RandomRock ->
        {
        Id = System.Guid.NewGuid();
        Name = "Rock";
        Wearing = { defaultWearing with InHand = true };
        Offence = Value((decimal)(rnd2 1 3));
        Defence = Value(0M);
        Type = Rock;
        MiscProperties = defaultMiscProperties
        }
    | CombatKnife ->
        {
        Id = System.Guid.NewGuid();
        Name = "Combat Knife";
        Wearing = { defaultWearing with InHand = true };
        Offence = Value((decimal)4);
        Defence = Value(0M);
        Type = Knife;
        MiscProperties = defaultMiscProperties
        }
    | OreExtractor ->
        { Id = Guid.NewGuid();
            Name = "Ore Extractor";
            Wearing = { defaultWearing with InHand = true };
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