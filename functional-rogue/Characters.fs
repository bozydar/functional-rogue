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
type Character (characterType: CharacterType, startingHP: int, startingDexterity) =

    let mutable hp : int = startingHP
    let mutable dexterity : int = startingDexterity

    member this.Type
        with get() = characterType

    member this.Dexterity 
        with get() : int = dexterity
        and set(value) = dexterity <- value
    
    member this.CurrentHP 
        with get() : int = hp
        and set(value) = hp <- value

    abstract MaxHP : int with get

    abstract GetMeleeDamage : damage with get
    
    abstract SightRadius : int with get

    abstract Appearance : char with get

    member this.IsAlive
        with get() : bool = this.CurrentHP > 0    

    abstract Name : string with get

    member this.HitWithDamage (value : int, attacker : Character)  =
        this.CurrentHP <- this.CurrentHP - value
        // add communicatio with state