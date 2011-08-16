module Monsters

type MonsterType =
    | Rat

    

type Monster (monsterType: MonsterType) =
    let guid = System.Guid.NewGuid

    let getMonsterAppearance =
        match monsterType with
        | Rat -> 'r'
    
    let getMonsterSightRadius =
        match monsterType with
        | Rat -> 2

    member this.Type
        with get() = monsterType

    member this.Guid
        with get() = guid

    member this.Appearance
        with get() = getMonsterAppearance
    
    member this.SightRadius
        with get() = getMonsterSightRadius

let createNewMonster (monsterType: MonsterType) : Monster =
    match monsterType with
    | Rat -> new Monster(Rat)