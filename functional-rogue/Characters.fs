module Characters

type damage = (int * int * int)

let scratchWound = 1
let lightWound = 3
let heavyWound = 9
let criticalWound = 27


type CharacterType = 
    | Avatar
    | Monster
    | NPC

[<AbstractClass>]
type Character (characterType: CharacterType, startingHP: int, startingDexterity : int, startingSightRadius : int) =
    let id = System.Guid.NewGuid ()
    let mutable hp = startingHP
    let mutable maxHP = startingHP
    let mutable sightRadius = startingSightRadius
    let mutable dexterity = startingDexterity

    member this.Type
        with get() = characterType

    member this.Dexterity 
        with get() = dexterity
        and set(value) = dexterity <- value
    
    member this.CurrentHP 
        with get() = hp
        and set(value) = hp <- value

    member this.MaxHP 
        with get() = maxHP
        and set(value) = maxHP <- value

    abstract GetMeleeDamage : damage with get
    
    member this.SightRadius  
        with get() = sightRadius
        and set(value) = sightRadius <- value

    abstract Appearance : char with get

    member this.IsAlive
        with get() : bool = this.CurrentHP > 0    

    abstract Name : string with get

    member this.HitWithDamage (value : int, attacker : Character)  =
        this.CurrentHP <- this.CurrentHP - value
        // add communicatio with state