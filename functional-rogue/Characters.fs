module Characters

type CharacterType = 
    | Avatar
    | Monster
    | NPC

[<AbstractClass>]
type Character (characterType: CharacterType)=

    member this.Type
        with get() = characterType

    abstract CurrentHP : int with get

    abstract MaxHP : int with get

    abstract GetMeleeDamage : int with get
    
    abstract SightRadius : int with get

    abstract Appearance : char with get

    abstract IsAlive : bool with get

    abstract HitWithDamage : int -> unit

    abstract Name : string with get