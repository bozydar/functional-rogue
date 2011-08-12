module AI

open State
open Monsters
open Board
open System.Drawing

let performRandomMovement (monsterPlace: (Point*Place)) (state:State) : State =
    let mutable possibleNewLocations = []
    let x = (fst monsterPlace).X
    let y = (fst monsterPlace).Y
    for tmpx in (max (x - 1) 0)..(min (x + 1) (boardWidth - 1)) do
        for tmpy in (max (y - 1) 0)..(min (y + 1) (boardHeight - 1)) do
            if (not(tmpx = x && tmpy = y) && not(isObstacle state.Board (Point(tmpx,tmpy)))) then
                possibleNewLocations <- possibleNewLocations @ [Point(tmpx,tmpy)]
    let resultState = { state with Board = state.Board |> Board.moveCharacter (snd monsterPlace).Character.Value (possibleNewLocations.[rnd possibleNewLocations.Length]) }
    resultState

let handleSingleMonster (monsterPlace: (Point*Place)) (state:State) : State =
    match (snd monsterPlace).Character.Value.Monster.Value.Type with
    | Rat -> performRandomMovement monsterPlace state

let handleMonsters (state: State) : State =
    let rec recursivelyHandleMonstersSequence (monsterPlaces: (Point*Place) list) (state:State) : State =
        match monsterPlaces with
        | head :: tail -> recursivelyHandleMonstersSequence tail (state |> handleSingleMonster head)
        | [] -> state

    let allMonsterPlaces = monsterPlaces state.Board
    //TO DO: here sort them by initiative
    state |> recursivelyHandleMonstersSequence allMonsterPlaces