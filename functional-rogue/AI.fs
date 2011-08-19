module AI

open State
open Monsters
open Board
open System.Drawing
open System.Collections.Generic

type GridPoint = {
        point : Point;
        mutable cameFrom : Point;
        mutable gScore : int;
        mutable hScore : int;
        mutable fScore : int
    }

// tools

let getDifferentSpeciesTheMonsterCanAttackInMelee (monsterPlace: (Point*Place)) (state:State) =
    let distance = 1
    let mutable result = []
    for x in (max 0 ((fst monsterPlace).X - distance))..(min (boardWidth - 1) ((fst monsterPlace).X + distance)) do
        for y in (max 0 ((fst monsterPlace).Y - distance))..(min (boardHeight - 1) ((fst monsterPlace).Y + distance)) do
            if( state.Board.[x,y].Character.IsSome &&
                (state.Board.[x,y].Character.Value.Type = CharacterType.Avatar
                    || state.Board.[x,y].Character.Value.Type = CharacterType.NPC
                    || (
                        state.Board.[x,y].Character.Value.Type = CharacterType.Monster &&
                        state.Board.[x,y].Character.Value.Monster.Value.Type <> (snd monsterPlace).Character.Value.Monster.Value.Type
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
    let sightRadius = (snd monsterPlace).Character.Value.Monster.Value.SightRadius
    let center = fst monsterPlace
    for x in (max 1 (center.X - sightRadius))..(min (center.X + sightRadius) (boardWidth - 2)) do
        for y in (max 1 (center.Y - sightRadius))..(min (center.Y + sightRadius) (boardHeight - 2)) do
            if not(isObstacle state.Board (Point(x,y))) || (fst monsterPlace) = (Point(x,y)) then
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
            

    let mutable resultList = []
    let mutable closedSet : GridPoint list = []
    let mutable openSet = [{ point = startPoint; cameFrom = startPoint; gScore = 0; hScore = (estimateCostToEnd startPoint endPoint); fScore = (estimateCostToEnd startPoint endPoint) }]
    while openSet.Length > 0 do
        openSet <- List.sortBy (fun element -> element.fScore) openSet
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
                    if (Point(x,y) = endPoint || not(isObstacle board (Point(x,y)))) && (not( listContains (Point(x,y)) closedSet )) then
                        let tentativeGScore = current.gScore + 1    //TODO: change this to something else later
                        if not( listContains (Point(x,y)) openSet) then
                            openSet <- openSet @ [{ point = Point(x,y); cameFrom = current.point; gScore = tentativeGScore; hScore = (estimateCostToEnd (Point(x,y)) endPoint); fScore = (tentativeGScore + (estimateCostToEnd (Point(x,y)) endPoint))}]
                        elif tentativeGScore > (List.find<GridPoint> (fun elem -> elem.point = Point(x,y)) openSet).gScore then
                            tentativeIsBetter <- false
                        
                        if tentativeIsBetter then
                            let elementIndex = List.findIndex (fun elem -> true) openSet
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
            if (not(tmpx = x && tmpy = y) && not(isObstacle state.Board (Point(tmpx,tmpy)))) then
                possibleNewLocations <- possibleNewLocations @ [Point(tmpx,tmpy)]
    let resultState = { state with Board = state.Board |> Board.moveCharacter (snd monsterPlace).Character.Value (possibleNewLocations.[rnd possibleNewLocations.Length]) }
    resultState

let getDifferentSpeciesTheMonsterCanSee (monsterPlace: (Point*Place)) (state:State) =
    let distance = (snd monsterPlace).Character.Value.Monster.Value.SightRadius
    let mutable result = []
    for x in (max 0 ((fst monsterPlace).X - distance))..(min (boardWidth - 1) ((fst monsterPlace).X + distance)) do
        for y in (max 0 ((fst monsterPlace).Y - distance))..(min (boardHeight - 1) ((fst monsterPlace).Y + distance)) do
            if( state.Board.[x,y].Character.IsSome &&
                (state.Board.[x,y].Character.Value.Type = CharacterType.Avatar
                    || state.Board.[x,y].Character.Value.Type = CharacterType.NPC
                    || (
                        state.Board.[x,y].Character.Value.Type = CharacterType.Monster &&
                        state.Board.[x,y].Character.Value.Monster.Value.Type <> (snd monsterPlace).Character.Value.Monster.Value.Type
                        )
                )) then
                result <- result @ [Point(x,y)]
    result

let rec calculateDangerScore (place: Point) (enemies: Point list) =
        match enemies with
        | head :: tail -> 10 - (max (abs (head.X - place.X)) (abs (head.Y - place.Y))) + calculateDangerScore place tail
        | [] -> 0

let getSpotsWithDangerScore (enemies: Point list) (monsterPlace: Point) (state: State) =
    let mutable spots = []
    for x in ((monsterPlace).X - 1)..((monsterPlace).X + 1) do
        for y in ((monsterPlace).Y - 1)..((monsterPlace).Y + 1) do
            if not(isObstacle state.Board (Point(x,y))) then
                spots <- spots @ [(Point(x,y),(calculateDangerScore (Point(x,y)) enemies))]
    spots

// specific monster AIs

let aiCowardMonster (monsterPlace: (Point*Place)) (state:State) : State =
    let differentSpecies = getDifferentSpeciesTheMonsterCanSee monsterPlace state
    if (differentSpecies.Length > 0) then
        let sortedSpotsWithDistanceScore = List.sortBy (fun element -> (snd element)) (getSpotsWithDangerScore differentSpecies (fst monsterPlace) state)
        let resultState = { state with Board = state.Board |> Board.moveCharacter (snd monsterPlace).Character.Value (fst (sortedSpotsWithDistanceScore.Head)) }
        resultState
    else
        performRandomMovement monsterPlace state

let aiLurkerPredatorMonster (monsterPlace: (Point*Place)) (state:State) : State =
    let monster = (snd monsterPlace).Character.Value
    let monsterPoint = fst monsterPlace
    match monster.Monster.Value.State with
    | MonsterState.Default ->
        monster.Monster.Value.State <- MonsterState.Lurking
        monster.Monster.Value.HungerFactor <- rnd2 30 60
        state |> State.updateCharacter (snd monsterPlace).Character.Value monster
    | MonsterState.Lurking ->
        let positions = getGoodLurkingPositionsInSightSortedFromBest monsterPlace state
        let newHungerFactor = monster.Monster.Value.HungerFactor - 1
        monster.Monster.Value.HungerFactor <- newHungerFactor
        if (newHungerFactor < 0) then
            monster.Monster.Value.State <- MonsterState.Hunting
            state |> State.updateCharacter (snd monsterPlace).Character.Value monster
        else
            let newState = state |> State.updateCharacter (snd monsterPlace).Character.Value monster
            if(positions.Length > 0) then
                if ((fst positions.Head) <> monsterPoint) then
                    newState |> goTowards monsterPlace (fst (positions.Head))
                else
                    newState
            else
                performRandomMovement monsterPlace newState
    | MonsterState.Hunting ->
        let differentSpecies = getDifferentSpeciesTheMonsterCanSee monsterPlace state
        let differentSpeciesWithDistance = differentSpecies |> List.map<Point,(Point*int)> (fun v -> (v,max (abs (monsterPoint.X - v.X)) (abs (monsterPoint.Y - v.Y))) )
        let sortedDiffSpeciesWithDist = List.sortBy (fun element -> (snd element)) differentSpeciesWithDistance
        let victims = getDifferentSpeciesTheMonsterCanAttackInMelee monsterPlace state
        if (victims.Length > 0) then
            state   //attack
        elif (sortedDiffSpeciesWithDist.Length > 0) then
            state
            |> State.updateCharacter (snd monsterPlace).Character.Value monster
            |> goTowards monsterPlace (fst (sortedDiffSpeciesWithDist.Head))
        else
            state
    | _ -> state

// some top level functions

let handleSingleMonster (monsterPlace: (Point*Place)) (state:State) : State =
    match (snd monsterPlace).Character.Value.Monster.Value.Type with
    | Rat -> aiCowardMonster monsterPlace state
    | Lurker -> aiLurkerPredatorMonster monsterPlace state

let handleMonsters (state: State) : State =
    let rec recursivelyHandleMonstersSequence (monsterPlaces: (Point*Place) list) (state:State) : State =
        match monsterPlaces with
        | head :: tail -> recursivelyHandleMonstersSequence tail (state |> handleSingleMonster head)
        | [] -> state

    let allMonsterPlaces = monsterPlaces state.Board
    //TO DO: here sort them by initiative
    state |> recursivelyHandleMonstersSequence allMonsterPlaces