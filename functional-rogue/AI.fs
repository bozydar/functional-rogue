﻿module AI

open State
open Monsters
open Board
open Characters
open Turn
open System.Drawing
open System.Collections.Generic
open Sight

type GridPoint = {
        point : Point;
        mutable cameFrom : Point;
        mutable gScore : int;
        mutable hScore : int;
        mutable fScore : int
    }

// tools

let AddMessageIfPlayerCanSeePoint (targetPoint : Point) (message : string) (state : State) = 
    if (Sight.canSee state.Board (getPlayerPosition state.Board) targetPoint) then
                State.addMessage (message) state
            else
                state

let getDifferentSpeciesTheMonsterCanAttackInMelee (monsterPlace: (Point*Place)) (state:State) =
    let distance = 1
    let monster = (snd monsterPlace).Character.Value :?> Monster
    let mutable result = []
    for x in (max 0 ((fst monsterPlace).X - distance))..(min (boardWidth - 1) ((fst monsterPlace).X + distance)) do
        for y in (max 0 ((fst monsterPlace).Y - distance))..(min (boardHeight - 1) ((fst monsterPlace).Y + distance)) do
            if( state.Board.Places.[x,y].Character.IsSome &&
                (state.Board.Places.[x,y].Character.Value.Type = CharacterType.Avatar
                    || state.Board.Places.[x,y].Character.Value.Type = CharacterType.NPC
                    || (
                        state.Board.Places.[x,y].Character.Value.Type = CharacterType.Monster &&
                        (state.Board.Places.[x,y].Character.Value :?> Monster).Type <> monster.Type
                        )
                )) then
                result <- result @ [Point(x,y)]
    result

let getGoodLurkingPositionsInSightSortedFromBest (monsterPlace: (Point*Place)) (state:State) =
    let calculateDistanceBonus (myPoint: Point) (target: Point) =
        let distance = max (abs (myPoint.X - target.X)) (abs (myPoint.Y - target.Y))
        match distance with
        | 0 -> 2
        | 1 -> 1
        | _ -> 0

    let mutable result = []
    let sightRadius = (snd monsterPlace).Character.Value.SightRadius
    let center = fst monsterPlace
    for x in (max 1 (center.X - sightRadius))..(min (center.X + sightRadius) (boardWidth - 2)) do
        for y in (max 1 (center.Y - sightRadius))..(min (center.Y + sightRadius) (boardHeight - 2)) do
            if not(isMovementObstacle state.Board (Point(x,y))) || (fst monsterPlace) = (Point(x,y)) then
                let obstaclesCount = countObstaclesAroundPoint (Point(x,y)) state.Board
                if (obstaclesCount > 3 && obstaclesCount < 8) then
                    let bonus = calculateDistanceBonus center (Point(x,y))
                    result <- result @ [(Point(x,y),obstaclesCount + bonus)]
    result |> List.sortBy (fun element -> -(snd element))

let aStar (startPoint: Point) (endPoint: Point) (board: Board) : (Point list) =

    let listContains (point: Point) (lst: GridPoint list) : bool =
        List.exists<GridPoint> (fun elem -> elem.point.X = point.X && elem.point.Y = point.Y ) lst

    let estimateCostToEnd (startPoint: Point) (endPoint: Point) =
        max (abs (startPoint.X - endPoint.X)) (abs (startPoint.Y - endPoint.Y))
    
    let rec reconstructPath (completeSet: GridPoint list) (currentNode: GridPoint)=
        if(currentNode.point = currentNode.cameFrom) then
            [currentNode.point] 
        else
            reconstructPath completeSet (List.find<GridPoint> (fun elem -> elem.point = currentNode.cameFrom ) completeSet) @ [currentNode.point]
    
    let sortByFScoreAndRandomizeEquals (e1 : GridPoint) (e2 : GridPoint) =
            if e1.fScore = e2.fScore then
                (rnd 3) - 1
            else e1.fScore - e2.fScore        

    let mutable resultList = []
    let mutable closedSet : GridPoint list = []
    let mutable openSet = [{ point = startPoint; cameFrom = startPoint; gScore = 0; hScore = (estimateCostToEnd startPoint endPoint); fScore = (estimateCostToEnd startPoint endPoint) }]
    while openSet.Length > 0 do
        openSet <- List.sortWith (fun e1 e2 -> sortByFScoreAndRandomizeEquals e1 e2) openSet
        let current = openSet.Head
        if (current.point = endPoint) then
            resultList <- reconstructPath closedSet current
            openSet <- [] //to finish the loop
        else
            openSet <- openSet.Tail
            closedSet <- closedSet @ [current]
            for x in (max 0 (current.point.X - 1))..(min (current.point.X + 1) (boardWidth - 1)) do
                for y in (max 0 (current.point.Y - 1))..(min (current.point.Y + 1) (boardHeight - 1)) do
                    let mutable tentativeIsBetter = true
                    if (Point(x,y) = endPoint || not(isMovementObstacle board (Point(x,y)))) && (not( listContains (Point(x,y)) closedSet )) then
                        let tentativeGScore = current.gScore + 1    //TODO: change this to something else later (terrain difficulty, etc.)
                        if not( listContains (Point(x,y)) openSet) then
                            openSet <- openSet @ [{ point = Point(x,y); cameFrom = current.point; gScore = tentativeGScore; hScore = (estimateCostToEnd (Point(x,y)) endPoint); fScore = (tentativeGScore + (estimateCostToEnd (Point(x,y)) endPoint))}]
                        elif tentativeGScore > (List.find<GridPoint> (fun elem -> elem.point = Point(x,y)) openSet).gScore then
                            tentativeIsBetter <- false
                        
                        if tentativeIsBetter then
                            let elementIndex = List.findIndex (fun elem -> elem.point = (Point(x,y))) openSet
                            openSet.[elementIndex].cameFrom <- current.point
                            openSet.[elementIndex].gScore <- tentativeGScore
                            openSet.[elementIndex].hScore <- (estimateCostToEnd (Point(x,y)) endPoint)
                            openSet.[elementIndex].fScore <- (tentativeGScore + (estimateCostToEnd (Point(x,y)) endPoint))
    resultList

let goTowards (monsterPlace: (Point*Place)) (targetLocation: Point) (state:State) : State =
    let movementSteps = aStar (fst monsterPlace) targetLocation state.Board
    if(movementSteps.Length > 1) then   // >1 because the first point is the current one
        { state with Board = state.Board |> Board.moveCharacter (snd monsterPlace).Character.Value movementSteps.Tail.Head }
    else
        state

let performRandomMovement (monsterPlace: (Point*Place)) (state:State) : State =
    let mutable possibleNewLocations = []
    let x = (fst monsterPlace).X
    let y = (fst monsterPlace).Y
    for tmpx in (max (x - 1) 0)..(min (x + 1) (boardWidth - 1)) do
        for tmpy in (max (y - 1) 0)..(min (y + 1) (boardHeight - 1)) do
            if (not(tmpx = x && tmpy = y) && not(isMovementObstacle state.Board (Point(tmpx,tmpy)))) then
                possibleNewLocations <- possibleNewLocations @ [Point(tmpx,tmpy)]
    let resultState = { state with Board = state.Board |> Board.moveCharacter (snd monsterPlace).Character.Value (possibleNewLocations.[rnd possibleNewLocations.Length]) }
    resultState

let getPlacesTheMonsterCanSeeByDistance (monsterPlace: (Point*Place)) (filter: Monster -> Place -> bool) (state:State) =
    let distance = (snd monsterPlace).Character.Value.SightRadius
    let monster = (snd monsterPlace).Character.Value :?> Monster
    let monsterPoint = fst monsterPlace
    let visiblePoints = visiblePositions monsterPoint monster.SightRadius false state.Board
    let result = visiblePoints |> List.map (fun point -> (point,Board.get state.Board point)) |> List.filter (fun item -> (filter monster (snd item))) |> List.map (fun item -> fst item)
    result |> List.sortBy (fun elem -> (max (abs (monsterPoint.X - elem.X)) (abs (monsterPoint.Y - elem.Y))) )

let placeContainsDifferentSpeciesThanMonster (monster: Monster) (place: Place) =
    if ( place.Character.IsSome &&
            ( place.Character.Value.Type = CharacterType.Avatar
            || place.Character.Value.Type = CharacterType.NPC
            || (
                place.Character.Value.Type = CharacterType.Monster &&
                (place.Character.Value :?> Monster).Type <> monster.Type
               )
            )
       ) then 
        true
    else
        false

let getDifferentSpeciesTheMonsterCanSee (monsterPlace: (Point*Place)) (state:State) =
    let distance = (snd monsterPlace).Character.Value.SightRadius
    let monster = (snd monsterPlace).Character.Value :?> Monster
    let differentSpeciesByDist = getPlacesTheMonsterCanSeeByDistance monsterPlace (fun monster place -> placeContainsDifferentSpeciesThanMonster monster place) state
    let result = differentSpeciesByDist |> List.map (fun item -> (item,state.Board.Places.[item.X,item.Y].Character.Value))
    result

let rec calculateDangerScore (place: Point) (enemies: (Point*Character) list) =
        match enemies with
        | head :: tail -> 10 - (max (abs ((fst head).X - place.X)) (abs ((fst head).Y - place.Y))) + calculateDangerScore place tail
        | [] -> 0

let getSpotsWithDangerScore (enemies: (Point*Character) list) (monsterPlace: Point) (state: State) =
    let mutable spots = []
    for x in ((monsterPlace).X - 1)..((monsterPlace).X + 1) do
        for y in ((monsterPlace).Y - 1)..((monsterPlace).Y + 1) do
            if not(isMovementObstacle state.Board (Point(x,y))) then
                spots <- spots @ [(Point(x,y),(calculateDangerScore (Point(x,y)) enemies))]
    spots

// specific monster AIs
let sortByDangerAndRandomizeEquals (e1 : (Point*int)) (e2 : (Point*int)) =
            if snd e1 = snd e2 then
                (rnd 3) - 1
            else snd e1 - snd e2

let aiCowardMonster (monsterPlace: (Point*Place)) (state:State) : State =
    let differentSpecies = getDifferentSpeciesTheMonsterCanSee monsterPlace state
    if (differentSpecies.Length > 0) then
        let sortedSpotsWithDistanceScore = List.sortWith (fun e1 e2 -> sortByDangerAndRandomizeEquals e1 e2) (getSpotsWithDangerScore differentSpecies (fst monsterPlace) state)
        if sortedSpotsWithDistanceScore.Length > 0 then
            { state with Board = state.Board |> Board.moveCharacter (snd monsterPlace).Character.Value (fst (sortedSpotsWithDistanceScore.Head)) }
        else 
            state
    else
        performRandomMovement monsterPlace state

let aiLurkerPredatorMonster (monsterPlace: (Point*Place)) (state:State) : State =
    let monster = (snd monsterPlace).Character.Value :?> Monster
    let monsterPoint = fst monsterPlace
    match monster.State with
    | CharacterAiState.Default ->
        monster.State <- CharacterAiState.Lurking
        monster.HungerFactor <- rnd2 30 60
        state
    | CharacterAiState.Lurking ->
        let differentSpeciesByDist = getPlacesTheMonsterCanSeeByDistance monsterPlace (fun monster place -> placeContainsDifferentSpeciesThanMonster monster place) state
        let newHungerFactor = monster.HungerFactor - 1
        monster.HungerFactor <- newHungerFactor
        if (newHungerFactor < 0) then
            monster.State <- CharacterAiState.Hunting
            state |> AddMessageIfPlayerCanSeePoint monsterPoint (monster.Name + " started looking for a prey")
        elif (differentSpeciesByDist.Length > 0 && (pointsDistance monsterPoint differentSpeciesByDist.Head) < 2) then
            monster.State <- CharacterAiState.Hunting
            state |> AddMessageIfPlayerCanSeePoint monsterPoint (monster.Name + " got annoyed by an intruder and is getting ready to attack")
        else
            let positions = getGoodLurkingPositionsInSightSortedFromBest monsterPlace state
            let newState = state
            if(positions.Length > 0) then
                if ((fst positions.Head) <> monsterPoint) then
                    newState |> goTowards monsterPlace (fst (positions.Head))
                else
                    newState
            else
                performRandomMovement monsterPlace newState
    | CharacterAiState.Hunting ->
        let differentSpeciesByDist = getPlacesTheMonsterCanSeeByDistance monsterPlace (fun monster place -> placeContainsDifferentSpeciesThanMonster monster place) state
        let corpses = getPlacesTheMonsterCanSeeByDistance monsterPlace (fun monster place -> place.Items |> List.exists (fun item -> item.Type = Type.Corpse)) state
        let victims = getDifferentSpeciesTheMonsterCanAttackInMelee monsterPlace state
        state
        |>  if (victims.Length > 0) then
                Mechanics.meleeAttack monster state.Board.Places.[victims.Head.X,victims.Head.Y].Character.Value
            elif (differentSpeciesByDist.Length > 0) then
                goTowards monsterPlace (differentSpeciesByDist.Head)
            elif (corpses.Length > 0) then
                if (max (abs(corpses.Head.X - monsterPoint.X)) (abs(corpses.Head.Y - monsterPoint.Y))) = 0 then //eat the corpse
                    monster.State <- CharacterAiState.Lurking
                    monster.HungerFactor <- rnd2 30 60
                    let thisPlace = state.Board.Places.[monsterPoint.X, monsterPoint.Y]
                    let corpseIndex = thisPlace.Items |> List.findIndex (fun item -> item.Type = Type.Corpse)
                    let corpseName = thisPlace.Items.[corpseIndex].Name
                    state.Board.Places.[monsterPoint.X, monsterPoint.Y] <- { thisPlace with Items = thisPlace.Items |> List.removeAt corpseIndex }
                    AddMessageIfPlayerCanSeePoint monsterPoint (monster.Name + " has consumed the " + corpseName)
                else
                    goTowards monsterPlace (corpses.Head)   //go to the nearest corpse to eat it
            else
                self

// some top level functions

let handleSingleMonster (monsterPlace: (Point*Place)) (state:State) : State =
    if((snd monsterPlace).Character.Value.IsAlive) then
        let monster = (snd monsterPlace).Character.Value :?> Monster
        match monster.Type with
        | Rat -> aiCowardMonster monsterPlace state
        | Lurker -> aiLurkerPredatorMonster monsterPlace state
    else
        state

let handleMonsters (state: State) : State =
    let rec recursivelyHandleMonstersSequence (monsterPlaces: (Point*Place) list) (state:State) : State =
        match monsterPlaces with
        | head :: tail -> recursivelyHandleMonstersSequence tail (state |> handleSingleMonster head)
        | [] -> state

    let allMonsterPlaces = monsterPlaces state.Board
    //TO DO: here sort them by initiative
    state |> recursivelyHandleMonstersSequence allMonsterPlaces
