module Predefined.Items

open System
open Mechanics
open Characters

type PredefinedItems =
    | RandomStick
    | RandomRock
    | OreExtractor   
    | KnuckleDuster 
    | Knife

let createPredefinedItem predefinedItem =
    match predefinedItem with
    | RandomStick ->
        {
        Id = Guid.NewGuid();
        Name = "Stick";
        Wearing = Wearing.HandOnly;
        Type = Stick;
        MiscProperties = defaultMiscProperties;        
        Attack = Some(fun attacker _ _ -> { Damage = 10, 10, 10; AttackBonus = 0; DefenceBonus = 0 });
        }
    | KnuckleDuster ->
        {
        Id = Guid.NewGuid();
        Name = "Knuckle-Duster";
        Wearing = Wearing.HandOnly;
        Type = Stick;
        MiscProperties = defaultMiscProperties;
        Attack = Some(fun attacker _ _ -> 
            let result = { Damage = 0, 0, 0; AttackBonus = 0; DefenceBonus = 0 }
            if attacker.Strength < 10 then { result with Damage = scratchWound, lightWound, lightWound }
            elif attacker.Strength < 14 then { result with Damage = lightWound, lightWound, heavyWound }
            elif attacker.Strength < 18 then { result with Damage = lightWound, heavyWound, heavyWound }
            else { result with Damage = lightWound, heavyWound, criticalWound })
       }
    | Knife ->
        {
        Id = Guid.NewGuid();
        Name = "Knife";
        Wearing = Wearing.HandOnly;
        Type = Stick;
        MiscProperties = defaultMiscProperties;
        Attack = Some(fun attacker _ _ -> 
            let result = { Damage = 0, 0, 0; AttackBonus = 1; DefenceBonus = 1 }
            if attacker.Strength < 10 then { result with Damage = scratchWound, lightWound, lightWound }
            elif attacker.Strength < 14 then { result with Damage = lightWound, lightWound, heavyWound }
            else { result with Damage = lightWound, heavyWound, heavyWound })
        }
    | RandomRock ->
        {
        Id = System.Guid.NewGuid();
        Name = "Rock";
        Wearing = Wearing.HandOnly;
        Type = Rock;
        MiscProperties = defaultMiscProperties
        Attack = None
        }
    | OreExtractor ->
        { Id = Guid.NewGuid();
            Name = "Ore Extractor";
            Wearing = Wearing.HandOnly;
            Type = Tool;
            MiscProperties = { defaultMiscProperties with OreExtractionRate = 1 };
            Attack = None
        }   

let stickOfDoom = {
        Id = System.Guid.NewGuid();
        Name = "Stick of doom";
        Wearing = Wearing.HandOnly;
        Type = Stick;
        MiscProperties = Characters.defaultMiscProperties;
        Attack = Some(fun attacker _ _ -> { Damage = lightWound, lightWound, lightWound; AttackBonus = 0; DefenceBonus = 0 });
    }

let createRandomNaturalItem (playerLevel: int) =
    //later player level will be used to determine probability of some more powerful/valuable items
    let randomResult = rnd 2
    if randomResult < 1 then
        createPredefinedItem RandomRock
    else
        createPredefinedItem RandomStick

