module Mechanics

open System
open Characters
open Board
open Log
open State
open Items

let roll () =
    rnd2 1 20

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
    [| 2; 0; -2; -5; -8; -11; -15; -20; -24 |].[inBoundary 0 9 level]
    
    
/// Returns number of successes 
let countableTest parameter bonus level =
    let dice = [roll (); roll (); roll ()] |> List.sort |> List.toSeq 
    let modifier = levelToModifier level
    let k = parameter + modifier
    if k > 0 then
        let results = dice |> Seq.scan (fun bonus item -> bonus - Math.Max(0, item - k)) bonus |> Seq.skip 1 
        // count all not negative items
        results |> Seq.sumBy (fun item -> if item >= 0 then 1 else 0)
    else
        0

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

let private evalMeleeDamage (attacker : Character) (defender : Character) = 
    let forAttacker = countableTest (attacker.Dexterity) 0 1
    let forDefender = countableTest (defender.Dexterity) 0 1
    let delta = forAttacker - forDefender
        
    if delta > 0 then 
        let damage = intByIndex (attacker.GetMeleeDamage) (delta - 1)
        damage
        //defender.HitWithDamage(damage, attacker)
    else 
        0

let killCharacter (victim: Character) (state: State) =
    let allBoardPlaces = places (state.Board)
    let victimPlace = Seq.find (fun x -> (snd x).Character = Some(victim)) allBoardPlaces
    let corpseItem = {
        Id = Guid.NewGuid();
        Name = victim.Name + " corpse";
        Wearing = {
                    OnHead = false;
                    InHand = false;
                    OnTorso = false;
                    OnLegs = false
        };
        Offence = Value(0M);
        Defence = Value(0M);
        Type = Corpse;
        MiscProperties = Items.defaultMiscProperties
        }
    { state with 
        Board = state.Board
        |> modify (fst victimPlace) (fun place -> { place with Character = option.None })
        |> modify (fst victimPlace) (fun place -> { place with Items = corpseItem :: place.Items} ) }
    |> addMessage (victim.Name + " has died.")

let meleeAttack (attacker: Character) (defender: Character) (state: State) =        
    let allBoardPlaces = places state.Board
    let attackerPlace = Seq.find (fun x -> (snd x).Character = Some(attacker)) allBoardPlaces
    let defenderPlace = Seq.find (fun x -> (snd x).Character = Some(defender)) allBoardPlaces
    //check if distance = 1
    if max (abs ((fst attackerPlace).X - (fst defenderPlace).X)) (abs ((fst attackerPlace).Y - (fst defenderPlace).Y)) = 1 then        
        let attackDamage = evalMeleeDamage attacker defender                   
        defender.HitWithDamage(attackDamage)

        state
        |> State.addMessage (defender.Name + " has got " + attackDamage.ToString() + " of damage.")
        |>  if defender.IsAlive then
                let defendDamage = evalMeleeDamage defender attacker     
                attacker.HitWithDamage defendDamage                 
                State.addMessage (attacker.Name + " has got " + defendDamage.ToString() + " of damage")
            else                    
                killCharacter defender
        |>  if not attacker.IsAlive then
                killCharacter attacker
            else
                self
    else
        state
