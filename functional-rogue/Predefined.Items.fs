namespace FunctionalRogue.Predefined

module Items =

    open FunctionalRogue
    open System
    open Characters

    let createCorpse (name: string) = 
        new Item(name + " corpse", Wearing.NotWearable, Corpse, Option.None)    


        
    let createKnuckleDuster () =
        new Item("Knuckle-Duster", Wearing.InHand, Stick, Some(fun attacker _ _ -> 
                let result = { Damage = 0, 0, 0; AttackBonus = 0; DefenceBonus = 0 }
                if attacker.Strength < 10 then { result with Damage = scratchWound, lightWound, lightWound }
                elif attacker.Strength < 14 then { result with Damage = lightWound, lightWound, heavyWound }
                elif attacker.Strength < 18 then { result with Damage = lightWound, heavyWound, heavyWound }
                else { result with Damage = lightWound, heavyWound, criticalWound }))
       
    let createKnife () =
        new Item("Knife", Wearing.InHand, Type.Knife, Some(fun attacker _ _ -> 
                let result = { Damage = 0, 0, 0; AttackBonus = 1; DefenceBonus = 1 }
                if attacker.Strength < 10 then { result with Damage = scratchWound, lightWound, lightWound }
                elif attacker.Strength < 14 then { result with Damage = lightWound, lightWound, heavyWound }
                else { result with Damage = lightWound, heavyWound, heavyWound }))

       
    let createIronHelmet () =
        new Item("Iron Helmet", Wearing.OnHead, Type.Hat, Some(fun attacker _ _ -> { Damage = 0, 0, 0; AttackBonus = 0; DefenceBonus = 5}))



    (*
    { Id = Guid.NewGuid();
                Name = "Ore Extractor";
                Wearing = Wearing.HandOnly;
                Type = Tool;
                MiscProperties = { defaultMiscProperties with OreExtractionRate = 1 };
                Attack = None
            }   
    *)
    let createOreExtractor () =
        new Item("Ore Extractor", Wearing.InHand, OreExtractor {HarvestRate = 1}, None)

    let stickOfDoom = Item("Stick of doom", Wearing.InHand, Stick, Some(fun attacker _ _ -> 
        { Damage = lightWound, lightWound, lightWound; AttackBonus = 0; DefenceBonus = 0 }))


    // LIQUID CONTAINERS
    let createEmptyMedicalInjector () =
        new Item("Medical Injector", Wearing.NotWearable, Type.Injector, None, Some({ LiquidCapacity = 100.0<ml>; LiquidInside = None }))

    let createEmptyCanteen () =
        new Item("Canteen", Wearing.NotWearable, Type.SimpleContainer, None, Some({ LiquidCapacity = 500.0<ml>; LiquidInside = None }))

    let fillContainerWithLiquid (liquidType : LiquidType) (containerItem : Item) =
        if containerItem.IsLiquidContainer then
            ignore (containerItem.AddLiquid {Type = liquidType; Amount = (containerItem.Container.Value.LiquidCapacity)})
        containerItem

    // DRONES

    let createReconnaissanceDrone () =
        new Item("Reconnaissance Drone", Wearing.NotWearable, Drone, Option.None)

    // NATURAL ITEMS

    let createRandomRock () =
        new Item("Rock", Wearing.InHand, Rock, None)

    let createStick () =
        new Item("Stick", Wearing.InHand, Stick, Some(fun attacker _ _ -> { Damage = 10, 10, 10; AttackBonus = 0; DefenceBonus = 0 }))

    let createRandomNaturalItem (playerLevel: int) =
        //later player level will be used to determine probability of some more powerful/valuable items
        let randomResult = rnd 2
        if randomResult < 1 then
            createRandomRock ()
        else
            createStick ()


