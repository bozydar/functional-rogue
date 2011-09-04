module Mechanics

open System

let dice () =
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
    
let simpleTest parameter bonus level =
    // get two lowest results
    let dices = [dice (); dice (); dice ()] |> List.sort |> List.toSeq |> Seq.take 2 
    let modifier = levelToModifier level
    let k = parameter + modifier
    if k > 0 then
        // check if can be lowered by bonus
        let overNormal = Seq.sumBy (fun item -> System.Math.Max(0, (item - k))) dices
        overNormal <= bonus
    else
        false

let openTest parameter bonus =
    // get two lowest results
    let dices = [dice (); dice (); dice ()] |> List.sort |> List.toSeq |> Seq.take 2 |> Seq.toList
    let k = parameter
    if k > 0 then
        // decrese the best:
        let toDecrease = Math.Max(0, dices.[0] - k)
        let rest = bonus - toDecrease
        let result = dices.[1] - rest
        Math.Max(-1, k - result)
    else
        -1

let rec oposedTest parameter1 bonus1 parameter2 bonus2 =
    let left = openTest parameter1 bonus1 
    let right = openTest parameter2 bonus2
    if left > right then 1
    elif left < right then -1
    else oposedTest parameter1 bonus1 parameter2 bonus2