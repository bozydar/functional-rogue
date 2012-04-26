module Predefined.Items

open System
open Characters

let corpse (name: string) = 
    Item(name + " corpse", Wearing.NotWearable, Corpse, Option.None)    

let stick = 
        Item("Stick", Wearing.HandOnly, Stick, Some(fun attacker _ _ -> { Damage = 10, 10, 10; AttackBonus = 0; DefenceBonus = 0 }))
        
let knuckleDuster = Item("Knuckle-Duster", Wearing.HandOnly, Stick, Some(fun attacker _ _ -> 
            let result = { Damage = 0, 0, 0; AttackBonus = 0; DefenceBonus = 0 }
            if attacker.Strength < 10 then { result with Damage = scratchWound, lightWound, lightWound }
            elif attacker.Strength < 14 then { result with Damage = lightWound, lightWound, heavyWound }
            elif attacker.Strength < 18 then { result with Damage = lightWound, heavyWound, heavyWound }
            else { result with Damage = lightWound, heavyWound, criticalWound }))
       
let knife = Item("Knife", Wearing.HandOnly, Type.Knife, Some(fun attacker _ _ -> 
            let result = { Damage = 0, 0, 0; AttackBonus = 1; DefenceBonus = 1 }
            if attacker.Strength < 10 then { result with Damage = scratchWound, lightWound, lightWound }
            elif attacker.Strength < 14 then { result with Damage = lightWound, lightWound, heavyWound }
            else { result with Damage = lightWound, heavyWound, heavyWound }))
let randomRock = Item("Rock", Wearing.HandOnly, Rock, None)
       
let ironHelmet = Item("Iron Helmet", Wearing.HeadOnly, Type.Hat, Some(fun attacker _ _ -> { Damage = 0, 0, 0; AttackBonus = 0; DefenceBonus = 5}))

let emptyMedicalInjector = Item("Medical Injector", Wearing.NotWearable, Type.Injector, None, Some({ LiquidCapacity = 100.0<ml>; LiquidInside = None }))

let createMedicalInjectorWithLiquid (liquidType : LiquidType) =
    let injector = emptyMedicalInjector
    ignore (injector.AddLiquid {Type = liquidType; Amount = 100.0<ml>})
    injector

(*
{ Id = Guid.NewGuid();
            Name = "Ore Extractor";
            Wearing = Wearing.HandOnly;
            Type = Tool;
            MiscProperties = { defaultMiscProperties with OreExtractionRate = 1 };
            Attack = None
        }   
*)
let oreExtractor = Item("Ore Extractor", Wearing.HandOnly, OreExtractor {HarvestRate = 1}, None)

let stickOfDoom = Item("Stick of doom", Wearing.HandOnly, Stick, Some(fun attacker _ _ -> 
    { Damage = lightWound, lightWound, lightWound; AttackBonus = 0; DefenceBonus = 0 }))

let reconnaissanceDrone = Item("Reconnaissance Drone", Wearing.NotWearable, Drone, Option.None)

let createRandomNaturalItem (playerLevel: int) =
    //later player level will be used to determine probability of some more powerful/valuable items
    let randomResult = rnd 2
    if randomResult < 1 then
        randomRock
    else
        stick

