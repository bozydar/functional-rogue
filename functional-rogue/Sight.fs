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


// returns points visible by player
let visiblePlaces1 (where: Point) distance board = Seq.empty


    
// TODO: http://rlforj.svn.sourceforge.net/viewvc/rlforj/trunk/src/rlforj/los/ShadowCasting.java?revision=12&view=markup