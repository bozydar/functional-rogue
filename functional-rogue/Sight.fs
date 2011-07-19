module Sight

open System
open System.Drawing
open Board

type ArcPoint = 
    val Theta : float
    val Leading : float
    val Lagging : float    
    val X : int
    val Y : int
    
    interface IComparable<ArcPoint> with
        member this.CompareTo obj = 
            if this.Theta > obj.Theta then 1 else -1

    override this.Equals obj = 
        let item = obj :?> ArcPoint 
        item.Theta = this.Theta

    override this.GetHashCode()  = 
        [this.Theta; this.Leading].GetHashCode()

    override this.ToString() =
        sprintf "[%d, %d, %f, %f, %f]" this.X this.Y this.Theta this.Leading this.Lagging

    new(x: int, y: int) =         
        let angle dx dy = 
            let toDegrees rad = 
                rad * 180.0 / Math.PI

            let d = Math.Atan2(dx, dy) |> toDegrees
            let a = (360.0 - d) % 360.0
            if a < 0.0 then a + 360.0 else a

        let xf = Convert.ToDouble x
        let yf = Convert.ToDouble y
        let theta = angle xf yf
        if xf < 0.0 && yf < 0.0 then
            {
                X = x;
                Y = y;
                Theta = theta;
                Leading = angle (xf + 0.5) (yf - 0.5)
                Lagging = angle (xf - 0.5) (yf + 0.5)
            } 
        elif xf < 0.0 then
            {
                X = x;
                Y = y;                
                Theta = theta;
                Leading = angle (xf - 0.5) (yf - 0.5)
                Lagging = angle (xf + 0.5) (yf + 0.5)
            }
        elif yf > 0.0 then
            {
                X = x;
                Y = y;                
                Theta = theta;
                Leading = angle (xf - 0.5) (yf + 0.5)
                Lagging = angle (xf + 0.5) (yf - 0.5)
            }
        else 
            {
                X = x;
                Y = y;                
                Theta = theta;
                Leading = angle (xf + 0.5) (yf + 0.5)
                Lagging = angle (xf - 0.5) (yf - 0.5)
            }

let distance (p1: Point) (p2: Point) = 
    let dx = p1.X - p2.X 
    let dy = p1.Y - p2.Y
    (dx * dx) + (dy * dy)
    |> Convert.ToDouble
    |> Math.Sqrt


let circles = 
    let radiusSeq = seq {
        let radius = 40
        let origin = point 0 0
        for i in -radius..radius do
            for j in -radius..radius do
                let dist =
                    point i j
                    |> distance origin
                    |> Math.Floor
                    |> Convert.ToInt32
                if dist <= radius then yield (dist, new ArcPoint(i, j))
        }   
    radiusSeq
    |> Seq.groupBy (fun (dist, _) -> dist)
    // remove keys from value sequences 
    |> Seq.map (fun (key, value) -> (key, Seq.map (fun (key1, val1) -> val1) value))
    |> dict 

// returns points visible by player
let visiblePlaces (where: Point) distance board = seq {
    let rec loop r th1 th2 = seq {
        let circ = Seq.toArray circles.[r]
        let wasObstacle = ref false
        let foundClear = ref false
        for i in 0..circ.Length - 1 do
            let arcPoint = circ.[i]
            let position = point (arcPoint.X + where.X) (arcPoint.Y + where.Y)
            if not (boardContains position)  then
                wasObstacle := true
            else 
                let currentPoint = point arcPoint.X arcPoint.Y
                if not ((arcPoint.Lagging < th1 || arcPoint.Leading > th2) && arcPoint.Theta <> th1 && arcPoint.Theta <> th2) then                
                    yield position
                    let isObstacle = isObstacle board position
                    if isObstacle then
                        if not !wasObstacle then
                            if !foundClear then
                                let runStartTheta = th1
                                let runEndTheta = arcPoint.Leading
                                if r < distance then
                                    yield! loop (r + 1) runStartTheta runEndTheta
                                wasObstacle := true
                            else
                                let nTh1 = if arcPoint.Theta = 0.0 then 0.0 else arcPoint.Leading
                                if r < distance then
                                    yield! loop (r + 1) nTh1 th2
                    else
                        foundClear := true
                        if !wasObstacle then
                            let last = circ.[i - 1]
                            if r < distance then
                                    yield! loop (r + 1) last.Theta th2
                        else
                            wasObstacle := false
                    wasObstacle := isObstacle
    }
    yield! loop 1 0.0 359.0
}

    
// TODO: http://rlforj.svn.sourceforge.net/viewvc/rlforj/trunk/src/rlforj/los/ShadowCasting.java?revision=12&view=markup