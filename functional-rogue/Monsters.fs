module Monsters

open Characters

type MonsterType =
    | Rat
    | Lurker

type CharacterAiState =
    | Lurking
    | Hunting
    | Default   //this is used for stateless monsters and for state monsters indicates that the monster has just been created and needs a setup

type Monster (monsterType: MonsterType, hp: int) =
    inherit Character (CharacterType.Monster)

    let mutable state = CharacterAiState.Default

    let mutable hungerFactor = 0

    let mutable hP = hp

    let maxHp = hp

    member this.Type
        with get() = monsterType

    //member this.Guid
    //    with get() = guid

    override this.Appearance
        with get() =
            match monsterType with
            | Rat -> 'r'
            | Lurker -> 'l'

    override this.Name 
        with get() =
            match monsterType with
            | Rat -> "Rat"
            | Lurker -> "Lurker"

    override this.CurrentHP
        with get() = hP

    override this.GetMeleeDamage
        with get() = 4
    
    override this.SightRadius
        with get() =
            match monsterType with
            | Rat -> 2
            | Lurker -> 5

    member this.State
        with get() = state
        and set(value) = state <- value

    member this.HungerFactor
        with get() = hungerFactor
        and set(value) = hungerFactor <- value

    override this.IsAlive
        with get() = hP > 0

    override this.HitWithDamage (damage: int) = 
        hP <- hP - damage

    override this.MaxHP
        with get() = maxHp

let createNewMonster (monsterType: MonsterType) : Monster =
    match monsterType with
    | Rat -> new Monster(Rat, 5)
    | Lurker -> new Monster(Lurker, 20)