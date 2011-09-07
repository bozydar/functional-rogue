module Monsters

open Characters

type MonsterType =
    | Rat
    | Lurker

type CharacterAiState =
    | Lurking
    | Hunting
    | Default   //this is used for stateless monsters and for state monsters indicates that the monster has just been created and needs a setup

type Monster (monsterType : MonsterType, hp : int, dexterity : int) =
    inherit Character (CharacterType.Monster, hp, dexterity)

    let mutable state = CharacterAiState.Default

    let mutable hungerFactor = 0

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


    override this.GetMeleeDamage 
        with get() : damage = scratchWound, scratchWound, scratchWound
    
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

    override this.MaxHP
        with get() = maxHp

let createNewMonster (monsterType: MonsterType) : Monster =
    match monsterType with
    | Rat -> new Monster(Rat, 5, 3)
    | Lurker -> new Monster(Lurker, 20, 4)