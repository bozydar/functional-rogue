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