module Mechanics

open System
open Characters
open Board
open Log
open State
open Predefined.Items

let roll () =
    rnd2 1 21

let percentToLevel (percent : int) : int =     
    let result = 
        if percent < 0 then 0
        elif 0 <= percent && percent < 10 then 1
        elif percent < 30 then 2
        elif percent < 60 then 3
        elif percent < 90 then 4
        elif percent < 120 then 5
        elif percent < 160 then 6
        elif percent < 200 then 7
        else 8
    result

let levelToModifier (level : int) = 
    [| 2; 0; -2; -5; -8; -11; -15; -20; -24 |].[inBoundary level 0 9]
    
let private countableTestWithDice parameter bonus level numberOfDice =
    if numberOfDice < 1 then 
        0
    else
        let dice = [for i in 1..numberOfDice -> roll ()] |> List.sort |> List.toSeq 
        let modifier = levelToModifier level
        let k = parameter + modifier
        if k > 0 then
            let results = dice |> Seq.scan (fun bonus item -> bonus - Math.Max(0, item - k)) bonus |> Seq.skip 1 
            // count all non-negative items
            results |> Seq.sumBy (fun item -> if item >= 0 then 1 else 0)
        else
            0

/// Returns number of successes 
let countableTest parameter bonus level =
    countableTestWithDice parameter bonus level 3

/// Returns true for success for simple test
let simpleTest parameter bonus level =
    countableTest parameter bonus level >= 2

/// Returns open test result
let openTest parameter bonus =
    // get two lowest results
    let dice = [roll (); roll (); roll ()] |> List.sort |> List.toSeq |> Seq.take 2 |> Seq.toList
    let k = parameter
    if k > 0 then
        // decrese the best:
        let toDecrease = Math.Max(0, dice.[0] - k)
        let rest = bonus - toDecrease
        let result = dice.[1] - rest
        Math.Max(-1, k - result)
    else
        -1

/// Returns 1 if *1 wins and -1 if *2 wins. Retries for draws.
let rec oposedTest parameter1 bonus1 parameter2 bonus2 =
    let left = openTest parameter1 bonus1 
    let right = openTest parameter2 bonus2
    if left > right then 1
    elif left < right then -1
    else oposedTest parameter1 bonus1 parameter2 bonus2

let private evalDamage (attacker : Character) (defender : Character) forAttacker forDefender =
    let delta = forAttacker - forDefender
        
    if delta > 0 then 
        let damages = attacker.MeleeAttack.Damage
        let damage = intByIndex damages (delta - 1)
        damage
    else 
        0

let private evalMeleeDamage (attacker : Character) (defender : Character) = 
    let forAttacker = countableTest (attacker.Dexterity) attacker.MeleeAttack.AttackBonus 1
    let forDefender = countableTest (defender.Dexterity) defender.MeleeAttack.DefenceBonus 1
    evalDamage attacker defender forAttacker forDefender

/// Gives defeneder chance to defend. It is used for fights with many foe simultanously.
let private evalDefendMeleeDamage (attacker : Character) (defender : Character) = 
    let forAttacker = countableTest (attacker.Dexterity) attacker.MeleeAttack.AttackBonus 1
    let forDefender = countableTestWithDice (defender.Dexterity) defender.MeleeAttack.DefenceBonus 1 forAttacker
    evalDamage attacker defender forAttacker forDefender

let killCharacter (victim: Character) (state: State) =
    let allBoardPlaces = places (state.Board)
    let victimPlace = Seq.find (fun x -> (snd x).Character = Some(victim)) allBoardPlaces
    let corpseItem = corpse victim.Name
    { state with 
        Board = state.Board
        |> modify (fst victimPlace) (fun place -> { place with Character = option.None })
        |> modify (fst victimPlace) (fun place -> { place with Items = corpseItem :: place.Items} ) }    

let meleeAttack (attacker: Character) (defender: Character) (state: State) =            
    // attacker cannot take part in more than one fight per turn
    if attacker.InvolvedInFight then 
        state             
    else
        let allBoardPlaces = places state.Board
        let attackerPlace = Seq.find (fun x -> (snd x).Character = Some(attacker)) allBoardPlaces
        let defenderPlace = Seq.find (fun x -> (snd x).Character = Some(defender)) allBoardPlaces
        //check if distance = 1
        if max (abs ((fst attackerPlace).X - (fst defenderPlace).X)) (abs ((fst attackerPlace).Y - (fst defenderPlace).Y)) = 1 then     
            let attackDamage =
                if defender.InvolvedInFight then
                    evalDefendMeleeDamage attacker defender                   
                else
                    evalMeleeDamage attacker defender                                         
            defender.HitWithDamage(attackDamage)

            let addMessageCharacterAware message state =
                if attacker :? Player.Player || defender :? Player.Player then
                    State.addMessage message state
                else
                    State.addMessage "You've heard animal scream" state
                
            let result = 
                state
                |> addMessageCharacterAware (defender.Name + " has got " + attackDamage.ToString() + " of damage.")
                |>  if defender.IsAlive then
                        if not defender.InvolvedInFight then
                            let defendDamage = evalMeleeDamage defender attacker     
                            attacker.HitWithDamage defendDamage
                            addMessageCharacterAware (attacker.Name + " has got " + defendDamage.ToString() + " of damage during contrattack")
                        else
                            self
                    else                    
                        killCharacter defender
                        >> addMessageCharacterAware (defender.Name + " has died.")
                |>  if not attacker.IsAlive then
                        killCharacter attacker
                        >> addMessageCharacterAware (attacker.Name + " has died.")
                    else
                        self
            
            do defender.InvolvedInFight <- true
            do attacker.InvolvedInFight <- true
            result
        else
            state
