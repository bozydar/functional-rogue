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
        Type = Stick;
        MiscProperties = defaultMiscProperties;        
        Attack = Some(fun attacker _ _ -> (10, 10, 10));
        }
    | RandomRock ->
        {
        Id = System.Guid.NewGuid();
        Name = "Rock";
        Wearing = Wearing.HandOnly;
        Type = Rock;
        MiscProperties = defaultMiscProperties
        Attack = Some(fun attacker _ _ -> (1, 1, 1));
        }
    | OreExtractor ->
        { Id = Guid.NewGuid();
            Name = "Ore Extractor";
            Wearing = Wearing.HandOnly;
            Type = Tool;
            MiscProperties = { defaultMiscProperties with OreExtractionRate = 1 };
            Attack = Some(fun attacker _ _ -> (1, 1, 1));
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
        Type = Stick;
        MiscProperties = Characters.defaultMiscProperties;
        Attack = Some(fun attacker _ _ -> (1, 1, 1));
    }

let createRandomNaturalItem (playerLevel: int) =
    //later player level will be used to determine probability of some more powerful/valuable items
    let randomResult = rnd 2
    if randomResult < 1 then
        createPredefinedItem RandomRock
    else
        createPredefinedItem RandomStick

