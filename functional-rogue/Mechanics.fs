﻿module Mechanics

open System

let scratchWound = 1
let lightWound = 3
let heavyWound = 9
let criticalWound = 27

let roll () =
    rnd2 1 20

let percentToLevel (percent : int) : int =     
    let result = 
        if percent < 0 then 0
        elif 0 <= percent && percent < 10 then 1
        elif 10 <= percent && percent < 30 then 2
        elif 30 <= percent && percent < 60 then 3
        elif 60 <= percent && percent < 90 then 4
        elif 90 <= percent && percent < 120 then 5
        elif 120 <= percent && percent < 160 then 6
        elif 160 <= percent && percent < 200 then 7
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
        let results = dice |> Seq.scan (fun bonus item -> bonus - Math.Max(0, item - k)) bonus
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