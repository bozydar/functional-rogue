module Monsters

open Characters

type MonsterType =
    | Rat
    | Lurker

type CharacterAiState =
    | Lurking
    | Hunting
    | Default   //this is used for stateless monsters and for state monsters indicates that the monster has just been created and needs a setup

type Monster (monsterType : MonsterType, hp : int,  dexterity : int, strength : int, sightRadius : int) =
    inherit Character (CharacterType.Monster, hp, dexterity, strength, sightRadius)

    let mutable state = CharacterAiState.Default

    let mutable hungerFactor = 0

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

    override this.MeleeAttack 
        with get() : AttackResult = { Damage = scratchWound, scratchWound, scratchWound; AttackBonus = 0; DefenceBonus = 0 }
    
    member this.State
        with get() = state
        and set(value) = state <- value

    member this.HungerFactor
        with get() = hungerFactor
        and set(value) = hungerFactor <- value        

let createNewMonster (monsterType: MonsterType) : Monster =
    match monsterType with
    | Rat -> new Monster(Rat, 5, 10, 8, 2)
    | Lurker -> new Monster(Lurker, 20, 5, 12, 4)