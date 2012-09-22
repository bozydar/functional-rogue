namespace FunctionalRogue.Predefined

module Items =

    open FunctionalRogue
    open System
    open Characters

    let createCorpse (name: string) = 
        new Item(name + " corpse", "%", Wearing.NotWearable, Corpse, Option.None)    

    let createKnuckleDuster () =
        new Item("Knuckle-Duster", "/", Wearing.InHand, Weapon { 
            Attack = Attack (fun (attacker : Character) _ _ -> 
                    let result = { Damage = 0, 0, 0; AttackBonus = 0; DefenceBonus = 0 }
                    if attacker.Strength < 10 then { result with Damage = scratchWound, lightWound, lightWound }
                    elif attacker.Strength < 14 then { result with Damage = lightWound, lightWound, heavyWound }
                    elif attacker.Strength < 18 then { result with Damage = lightWound, heavyWound, heavyWound }
                    else { result with Damage = lightWound, heavyWound, criticalWound }); Type = Knife }, None)
       
    let createKnife () =
        new Item("Knife", "/", Wearing.InHand, Weapon { 
            Attack = Attack (fun (attacker : Character) _ _ -> 
                let result = { Damage = 0, 0, 0; AttackBonus = 1; DefenceBonus = 1 }
                if attacker.Strength < 10 then { result with Damage = scratchWound, lightWound, lightWound }
                elif attacker.Strength < 14 then { result with Damage = lightWound, lightWound, heavyWound }
                else { result with Damage = lightWound, heavyWound, heavyWound }); Type = Knife }, None)

       
    let createIronHelmet () =
        new Item("Iron Helmet", "]", Wearing.OnHead, Armor { 
            Defence = Attack (fun (attacker : Character) _ _ -> 
                { Damage = 0, 0, 0; AttackBonus = 0; DefenceBonus = 5})}, None)

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
        new Item("Ore Extractor", "_", Wearing.InHand, OreExtractor {HarvestRate = 1}, None)

    let stickOfDoom = Item("Stick of doom", "|", Wearing.InHand, Weapon { 
        Attack = Attack (fun _ _ _ ->  { Damage = lightWound, lightWound, lightWound; AttackBonus = 0; DefenceBonus = 0 }); Type = Stick }, None)

    // LIQUID CONTAINERS
    let createEmptyMedicalInjector () =
        new Item("Medical Injector", "!", Wearing.NotWearable, Type.Injector, Some({ LiquidCapacity = 100.0<ml>; LiquidInside = None }))

    let createEmptyCanteen () =
        new Item("Canteen", "!", Wearing.NotWearable, Type.SimpleContainer, Some({ LiquidCapacity = 500.0<ml>; LiquidInside = None }))

    let fillContainerWithLiquid (liquidType : LiquidType) (containerItem : Item) =
        if containerItem.IsLiquidContainer then
            ignore (containerItem.AddLiquid {Type = liquidType; Amount = (containerItem.Container.Value.LiquidCapacity)})
        containerItem

    // DRONES

    let createReconnaissanceDrone () =
        new Item("Reconnaissance Drone", "^", Wearing.NotWearable, Drone, Option.None)

    // NATURAL ITEMS

    let createRandomRock () =
        new Item("Rock", "*", Wearing.InHand, Rock, None)

    let createStick () =
        new Item("Stick", "|", Wearing.InHand, Weapon { 
        Attack = Attack (fun _ _ _ ->  { Damage = 10, 10, 10; AttackBonus = 0; DefenceBonus = 0 }); Type = Stick }, None)

    let createRandomNaturalItem (playerLevel: int) =
        //later player level will be used to determine probability of some more powerful/valuable items
        let randomResult = rnd 2
        if randomResult < 1 then
            createRandomRock ()
        else
            createStick ()


