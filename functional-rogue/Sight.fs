module Sight

open Utils
open System
open System.Drawing
open Board
open State

// returns points visible by player
let visiblePositions (where: Point) distance board = 
    let loop2 degrees = seq {
        for (degree: int) in degrees do
            let i = Convert.ToDouble(degree)
            let move = { X = Math.Cos(i * Math.PI / 180.0); Y = Math.Sin(i * Math.PI / 180.0)}

            let rec loop1 (fPoint: FloatingPoint) (j: int) = seq {            
                let p = fPoint.ToPoint
                if boardContains p then
                    yield p
                if isOpticalObstacle board p then 
                    ()
                elif j < distance then
                    yield! loop1 (fPoint + move) (j + 1)
            }

            yield! loop1 (move + where) 1}        
    
    where :: (loop2 [1..3..360]  
    |> Seq.distinct 
    |> Seq.toList)
    
let setVisibilityStates state  = 
    let playerPosition = getPlayerPosition state.Board
    let positions = visiblePositions playerPosition state.Player.SightRadius state.Board    
    let preResult = Array2D.mapi (fun x y place -> 
        let p = point x y
        if Seq.exists ((=) p) positions then {place with IsSeen = true; WasSeen = true} else {place with IsSeen = false}) state.Board
    {state with Board = preResult}
    
