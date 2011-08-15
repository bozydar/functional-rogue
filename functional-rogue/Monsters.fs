module Monsters

type MonsterType =
    | Rat

    

type Monster (monsterType: MonsterType) =
    let guid = System.Guid.NewGuid

    let getMonsterAppearance =
        match monsterType with
        | Rat -> 'r'

    member this.Type
        with get() = monsterType

    member this.Guid
        with get() = guid

    member this.Appearance
        with get() = getMonsterAppearance


let createNewMonster (monsterType: MonsterType) : Monster =
    match monsterType with
    | Rat -> new Monster(Rat)