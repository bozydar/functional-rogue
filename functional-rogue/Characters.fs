module Characters

open System

type damage = (int * int * int)

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
type Character (characterType: CharacterType, startingHP: int, startingDexterity : int, startingStrength : int, startingSightRadius : int) =
    let id = System.Guid.NewGuid ()
    let mutable hp = startingHP
    let mutable maxHP = startingHP
    let mutable sightRadius = startingSightRadius
    let mutable dexterity = startingDexterity
    let mutable strength = startingStrength

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
        with get() = sightRadius
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
and [<CustomEquality; CustomComparison>] Item = {    
    Id : Guid;
    Name : string;
    Wearing : Wearing
    Type : Type;
    MiscProperties : MiscProperties;
    // attacker -> defener -> distance -> (damage * attackBonus * defenceBonus)
    Attack : (Character -> Character -> int -> AttackResult) option  
} 
with 
    override this.Equals(other) =
        match other with
        | :? Item as other -> other.Id = this.Id
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
        item.Name 
    rest
