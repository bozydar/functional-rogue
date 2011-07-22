module Sight

open System
open System.Drawing
open Board

// returns points visible by player
let visiblePlaces (where: Point) distance board = seq {
    for j in 1..3..360  do
        let i = Convert.ToDouble(j)
        let dist = ref 0
        let ox = ref(Convert.ToDouble where.X)
        let oy = ref(Convert.ToDouble where.Y)
        let xMove = ref(Math.Cos(i * Math.PI / 180.0))
        let yMove = ref(Math.Sin(i * Math.PI / 180.0))
        let bre = ref false
        let j = ref 0
        while (not !bre) && !j <= distance do            
            let a = Convert.ToInt32(!ox)
            let b = Convert.ToInt32(!oy)            
            let p = point a b
            if boardContains p then
                yield p
            if isObstacle board p then 
                bre := true
            else
                ox := !ox + !xMove
                oy := !oy + !yMove
                j := !j + 1

    }
