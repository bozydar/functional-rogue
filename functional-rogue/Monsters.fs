module Monsters

type MonsterType =
    | Rat
    | Lurker

type MonsterState =
    | Lurking
    | Hunting
    | Default   //this is used for stateless monsters and for state monsters indicates that the monster has just been created and needs a setup

type Monster (monsterType: MonsterType, hp: int) =
    let guid = System.Guid.NewGuid

    let mutable state = MonsterState.Default

    let mutable hungerFactor = 0

    let mutable hP = hp

    let getMonsterSightRadius =
        match monsterType with
        | Rat -> 2
        | Lurker -> 5

    member this.Type
        with get() = monsterType

    member this.Guid
        with get() = guid

    member this.Appearance
        with get() =
            match monsterType with
            | Rat -> 'r'
            | Lurker -> 'l'

    member this.CurrentHP
        with get() = hP

    member this.GetMeleeDamage
        with get() = 4
    
    member this.SightRadius
        with get() = getMonsterSightRadius

    member this.State
        with get() = state
        and set(value) = state <- value

    member this.HungerFactor
        with get() = hungerFactor
        and set(value) = hungerFactor <- value

    member this.IsAlive
        with get() = hP > 0

    member this.HitWithDamage (damage: int) = 
        hP <- hP - damage

let createNewMonster (monsterType: MonsterType) : Monster =
    match monsterType with
    | Rat -> new Monster(Rat, 5)
    | Lurker -> new Monster(Lurker, 20)