module Characters

open System

type damage = (int * int * int)

[<Measure>] type ml

type AttackResult = {
    Damage : damage;
    AttackBonus : int;
    DefenceBonus : int
}

let scratchWound = 1
let lightWound = 3
let heavyWound = 9
let criticalWound = 27


type CharacterType = 
    | Avatar
    | Monster
    | NPC

[<AbstractClass>]
type Character (characterType: CharacterType, startingHP: int, startingDexterity : int, startingStrength : int, startingSightRadius : int, startingHungerFactorStep : int) =
    let id = System.Guid.NewGuid ()
    let mutable hp = startingHP
    let mutable maxHP = startingHP
    let mutable sightRadius = startingSightRadius
    let mutable dexterity = startingDexterity
    let mutable strength = startingStrength
    let mutable involvedInFight = false
    let mutable hungerFactor = 0
    let mutable hungerFactorStep = startingHungerFactorStep
    let mutable sightRadiusMultiplier = 1
    let mutable canSeeThroughWalls = false

    let mutable items : list<Item> = []

    let mutable wornItems : WornItems = { Head = Option.None; Hand = Option.None; Torso = Option.None; Legs = Option.None }

    member this.Items
        with get() = items
        and set(value) = items <- value

    member this.WornItems
        with get() = wornItems
        and set(value) = wornItems <- value


    member this.Type
        with get() = characterType

    member this.Dexterity 
        with get() = dexterity
        and set(value) = dexterity <- value

    member this.Strength
        with get() = strength
        and set(value) = strength <- value
    
    member this.CurrentHP 
        with get() = hp
        and set(value) = hp <- value

    member this.MaxHP 
        with get() = maxHP
        and set(value) = maxHP <- value

    member this.SightRadiusMultiplier
        with get() = sightRadiusMultiplier
        and set(value) = sightRadiusMultiplier <- value

    member this.CanSeeThroughWalls
        with get() = canSeeThroughWalls
        and set(value) = canSeeThroughWalls <- value

    member this.InvolvedInFight
        with get() = involvedInFight
        and set(value) = involvedInFight <- value

    member this.ResetVolatileStates () =
        this.InvolvedInFight <- false

    member this.TickBiologicalClock () =
        hungerFactor <- hungerFactor + 1
        match this.HungerLevel with
        | 1 when rnd 100 < 3 -> this.HitWithDamage 1
        | 2 when rnd 100 < 10   -> this.HitWithDamage 1
        | 3 when rnd 100 < 20   -> this.HitWithDamage 1
        | _ -> ()

    member this.HungerLevel =         
        if isInBoundary this.HungerFactor hungerFactorStep (hungerFactorStep * 2) then 1
        elif isInBoundary this.HungerFactor (hungerFactorStep * 2) (hungerFactorStep * 3) then 2
        elif this.HungerFactor > (hungerFactorStep * 3) then 3
        else 0

    member this.HungerFactor
        with get() = hungerFactor
        and set(value) = hungerFactor <- value

    member this.Eat (item : Item) =
        hungerFactor <- Math.Min(0, hungerFactor - if item.IsEatable then -100 else 0)

    abstract member MeleeAttack : AttackResult with get
    default this.MeleeAttack
        with get() =
            if this.WornItems.Hand.IsSome && this.WornItems.Hand.Value.Attack.IsSome 
            then this.WornItems.Hand.Value.Attack.Value this this 1
            else this.DefaultMeleeAttack
    
    abstract member DefaultMeleeAttack : AttackResult with get
    default this.DefaultMeleeAttack 
        with get() = 
            let result = { Damage = (0, 0, 0); AttackBonus = 0; DefenceBonus = 0 }
            if this.Strength < 10 then { result with Damage = scratchWound, scratchWound, scratchWound }
            elif this.Strength < 12 then { result with Damage = scratchWound, scratchWound, lightWound }
            elif this.Strength < 14 then { result with Damage = scratchWound, lightWound, lightWound }
            elif this.Strength < 16 then { result with Damage = scratchWound, lightWound, heavyWound }
            elif this.Strength < 18 then { result with Damage = lightWound, lightWound, heavyWound }
            else { result with Damage = lightWound, heavyWound, heavyWound }

    member this.SightRadius  
        with get() = sightRadius * sightRadiusMultiplier
        and set(value) = sightRadius <- value

    abstract Appearance : char with get

    member this.IsAlive
        with get() : bool = this.CurrentHP > 0    

    abstract Name : string with get

    member this.HitWithDamage (value : int)  =
        this.CurrentHP <- this.CurrentHP - value

and WornItems = {
    Head : option<Item>;
    Hand : option<Item>;    
    Torso : option<Item>;
    Legs : option<Item>
} 
and 
    Item (name: string, wearing: Wearing, _type: Type,
            attack : (Character -> Character -> int -> AttackResult) option,
            initialContainer : Container option ) =
    let id = Guid.NewGuid()
    let mutable container = initialContainer
    
    new (name: string, wearing: Wearing, _type: Type,
            attack : (Character -> Character -> int -> AttackResult) option) = Item(name, wearing, _type, attack, None) 

    member this.Id
        with get() : Guid = id 
   
    member this.Name
        with get() : string = name

    member this.Wearing 
        with get() : Wearing = wearing

    member this.Type
        with get() : Type = _type

    member this.Attack 
        with get() : (Character -> Character -> int -> AttackResult) option = attack

    member this.IsWearable
        with get() : bool = this.Wearing <> Wearing.NotWearable

    member this.IsEatable
        with get() : bool = this.Type = Type.Corpse

    //container section

    member this.IsLiquidContainer
        with get() : bool = container.IsSome && container.Value.LiquidCapacity > 0.0<ml>

    member this.Container
        with get() = container

    member this.AddLiquid (liquid : Liquid) =
        if this.IsLiquidContainer && (this.Container.Value.LiquidInside.IsNone || this.Container.Value.LiquidInside.Value.Type = liquid.Type) then
            let spaceAvailable = container.Value.LiquidCapacity - (if container.Value.LiquidInside.IsSome then container.Value.LiquidInside.Value.Amount else 0.0<ml>)
            let newLiquidInsideAmount = (if container.Value.LiquidInside.IsSome then container.Value.LiquidInside.Value.Amount else 0.0<ml>) + (if spaceAvailable < liquid.Amount then spaceAvailable else liquid.Amount)
            let newLiquidOutsideAmount = if spaceAvailable > liquid.Amount then 0.0<ml> else liquid.Amount - spaceAvailable
            let newLiquidInsideType = liquid.Type
            container <- Some( { container.Value with LiquidInside = Some( { Type = newLiquidInsideType; Amount = newLiquidInsideAmount} ) } )
            if newLiquidOutsideAmount > 0.0<ml> then Some({ Type = liquid.Type; Amount = newLiquidOutsideAmount}) else None
        else
            Some(liquid)

    member this.TakeLiquid (amount : float<ml>) =
        if this.IsLiquidContainer && container.Value.LiquidInside.IsSome then
            if amount > container.Value.LiquidInside.Value.Amount then
                let result = container.Value.LiquidInside
                container <- Some( {container.Value with LiquidInside = None} )
                result
            else
                let resultLiquid = { Type = container.Value.LiquidInside.Value.Type; Amount = amount }
                let liquidLeft = { Type = container.Value.LiquidInside.Value.Type; Amount = container.Value.LiquidInside.Value.Amount - amount }
                container <- Some( {container.Value with LiquidInside = if liquidLeft.Amount > 0.0<ml> then Some(liquidLeft) else None } )
                Some(resultLiquid)
        else
            None
    //end container section

    override this.Equals(other) =
        match other with
        | :? Item as other -> other.Id = id
        | _ -> false
    
    override this.GetHashCode() = 
        hash this.Id

    interface IComparable with
        member this.CompareTo(other) =
            match other with
            | :? Item as other -> compare this other
            | _ -> invalidArg "other" "cannot compare values of different types"        

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
    | Knife
    | Hat
    | Corpse
    | OreExtractor of OreExtractorProperties
    | Drone
    | Injector
    | SimpleContainer
and OreExtractorProperties = { HarvestRate: int }

and MiscProperties = {
    OreExtractionRate : int
}

and Liquid = {
    Type : LiquidType
    Amount : float<ml>
}

and LiquidType =
    | Water
    | HealingSolution

and Container = {
    LiquidCapacity : float<ml>
    LiquidInside : Liquid option
    //SubstanceCapacity : int
    //ItemsCapacity : int
    //Items : Item list
}

let defaultMiscProperties = {
    OreExtractionRate = 0
}

let itemShortDescription (item: Item)=
    let rest = 
        let liqiudContainerDescription =
            if item.IsLiquidContainer then 
                if item.Container.Value.LiquidInside.IsSome then
                    " (" + item.Container.Value.LiquidInside.Value.Amount.ToString() + "ml of " + (getUnionCaseName item.Container.Value.LiquidInside.Value.Type) + ")"
                else
                    "(empty)"
            else
                ""
        item.Name + liqiudContainerDescription
    rest
