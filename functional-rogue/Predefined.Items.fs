module Predefined.Items

open System
open Mechanics
open Characters

type PredefinedItems =
    | RandomStick
    | RandomRock
    | OreExtractor    

let createPredefinedItem predefinedItem =
    match predefinedItem with
    | RandomStick ->
        {
        Id = Guid.NewGuid();
        Name = "Stick";
        Wearing = Wearing.HandOnly;
        Offence = Value((decimal)(rnd2 1 3));
        Defence = Value(0M);
        Type = Stick;
        MiscProperties = defaultMiscProperties;        
        Attack = Some(fun attacker _ -> (10, 10, 10));
        }
    | RandomRock ->
        {
        Id = System.Guid.NewGuid();
        Name = "Rock";
        Wearing = Wearing.HandOnly;
        Offence = Value((decimal)(rnd2 1 3));
        Defence = Value(0M);
        Type = Rock;
        MiscProperties = defaultMiscProperties
        Attack = Some(fun attacker _ -> (1, 1, 1));
        }
    | OreExtractor ->
        { Id = Guid.NewGuid();
            Name = "Ore Extractor";
            Wearing = Wearing.HandOnly;
            Offence = Value(0M);
            Defence = Value(0M);
            Type = Tool;
            MiscProperties = { defaultMiscProperties with OreExtractionRate = 1 };
            Attack = Some(fun attacker _ -> (1, 1, 1));
        }   

let stickOfDoom = {
        Id = System.Guid.NewGuid();
        Name = "Stick of doom";
        Wearing = {
                    OnHead = false;
                    InHand = true;
                    OnTorso = false;
                    OnLegs = true
        };
        Offence = Value(3M);
        Defence = Value(0M);
        Type = Stick;
        MiscProperties = Characters.defaultMiscProperties;
        Attack = Some(fun attacker _ -> (1, 1, 1));
    }

let createRandomNaturalItem (playerLevel: int) =
    //later player level will be used to determine probability of some more powerful/valuable items
    let randomResult = rnd 2
    if randomResult < 1 then
        createPredefinedItem RandomRock
    else
        createPredefinedItem RandomStick

