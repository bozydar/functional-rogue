module Monsters

type MonsterType =
    | Rat
    | Lurker

type MonsterState =
    | Lurking
    | Hunting
    | Default   //this is used for stateless monsters and for state monsters indicates that the monster has just been created and needs a setup

type Monster (monsterType: MonsterType) =
    let guid = System.Guid.NewGuid

    let mutable state = MonsterState.Default

    let mutable hungerFactor = 0

    let getMonsterAppearance =
        match monsterType with
        | Rat -> 'r'
        | Lurker -> 'l'
    
    let getMonsterSightRadius =
        match monsterType with
        | Rat -> 2
        | Lurker -> 5

    member this.Type
        with get() = monsterType

    member this.Guid
        with get() = guid

    member this.Appearance
        with get() = getMonsterAppearance
    
    member this.SightRadius
        with get() = getMonsterSightRadius

    member this.State
        with get() = state
        and set(value) = state <- value

    member this.HungerFactor
        with get() = hungerFactor
        and set(value) = hungerFactor <- value

let createNewMonster (monsterType: MonsterType) : Monster =
    match monsterType with
    | Rat -> new Monster(Rat)
    | Lurker -> new Monster(Lurker)