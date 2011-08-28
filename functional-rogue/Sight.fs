module Sight

open Utils
open System
open System.Drawing
open Board
open State

// returns points visible by player
let _visiblePositions (where: Point) distance board = 
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

type private Line =         
    val mutable XI : int 
    val mutable YI : int 
    val mutable XF : int 
    val mutable YF : int 

    member this.DX 
        with get() = this.XF - this.XI

    member this.DY
        with get() = this.YF - this.YI        

    new(xi : int, yi : int, xf : int, yf : int) =
        {XI = xi;
        YI = yi;
        XF = xf;
        YF = yf}

    member this.RelativeSlope(x, y) =
        (this.DY * (this.XF - x)) - (this.DX * (this.YF - y))
        
    member this.Below(x, y) =
        this.RelativeSlope(x, y) > 0

    member this.BelowOrColinear(x, y) =
        this.RelativeSlope(x, y) >= 0

    member this.Above(x, y) =
        this.RelativeSlope(x, y) < 0

    member this.AboveOrColinear(x, y) =
        this.RelativeSlope(x, y) <= 0

    member this.Colinear(x, y) =
        this.RelativeSlope(x, y) = 0

    member this.LineColinear(line : Line) =
        this.Colinear(line.XI, line.YI) &&
        this.Colinear(line.XF, line.YF)
    
    interface ICloneable with 
        member this.Clone () = 
            new Line (this.XI, this.YI, this.XF, this.YF) :> obj


[<AllowNullLiteral>]
type ViewBump =    
    val mutable X : int
    val mutable Y : int
    val mutable Parent : ViewBump

    new(x, y, parent) =
        { X = x; Y = y; Parent = parent}    

    interface ICloneable with 
        member this.Clone () = 
            let parent = if this.Parent <> null then (this.Parent :> ICloneable).Clone() :?> ViewBump else null
            new ViewBump (this.X, this.Y, this.Parent) :> obj

type private View(shallowLine, steepLine) as this =
    [<DefaultValue>] val mutable ShallowBump : ViewBump 
    [<DefaultValue>] val mutable SteepBump : ViewBump 
    [<DefaultValue>] val mutable ShallowLine : Line
    [<DefaultValue>] val mutable SteepLine : Line
    do
        this.ShallowLine <- shallowLine
        this.SteepLine <- steepLine
        this.SteepBump <- Unchecked.defaultof<ViewBump>
        this.ShallowBump <- Unchecked.defaultof<ViewBump>        
    interface ICloneable with 
        member this.Clone () = 
            let shallowBump = if this.ShallowBump <> null then (this.ShallowBump :> ICloneable).Clone() :?> ViewBump else null
            let steepBump = if this.SteepBump <> null then (this.SteepBump :> ICloneable).Clone() :?> ViewBump else null
            let shallowLine = (this.ShallowLine :> ICloneable).Clone() :?> Line
            let steepLine = (this.SteepLine :> ICloneable).Clone() :?> Line
            let result = new View (shallowLine, steepLine)
            result.ShallowBump <- shallowBump
            result.SteepBump <- steepBump
            result :> obj

let private addShallowBump x y (activeViews : View list byref) viewIndex = 
    activeViews.[viewIndex].ShallowLine.XF <- x
    activeViews.[viewIndex].ShallowLine.YF <- y
    activeViews.[viewIndex].ShallowBump <- new ViewBump(x, y, activeViews.[viewIndex].ShallowBump)

    let mutable curBump = activeViews.[viewIndex].SteepBump
    while curBump <> null do
        if activeViews.[viewIndex].ShallowLine.Above(curBump.X, curBump.Y) then
            activeViews.[viewIndex].ShallowLine.XI <- curBump.X
            activeViews.[viewIndex].ShallowLine.YI <- curBump.Y
        curBump <- curBump.Parent

let private addSteepBump x y (activeViews : View list byref) viewIndex =
    activeViews.[viewIndex].SteepLine.XF <- x
    activeViews.[viewIndex].SteepLine.YF <- y
    activeViews.[viewIndex].SteepBump <- new ViewBump(x, y, activeViews.[viewIndex].SteepBump)    

    let mutable curBump = activeViews.[viewIndex].ShallowBump
    while curBump <> null do
        if activeViews.[viewIndex].SteepLine.Below(curBump.X, curBump.Y) then
            activeViews.[viewIndex].SteepLine.XI <- curBump.X
            activeViews.[viewIndex].SteepLine.YI <- curBump.Y
        curBump <- curBump.Parent

let private checkView (activeViews : View list byref) viewIndex =
    let shallowLine = activeViews.[viewIndex].ShallowLine
    let steepLine = activeViews.[viewIndex].SteepLine

    if shallowLine.LineColinear steepLine && (shallowLine.Colinear(0, 1) || shallowLine.Colinear(1, 0)) then
        activeViews <- List.removeAt viewIndex activeViews
        false
    else
        true

let private visitCoord (visited : (int*int) list byref) startX startY x y dx dy viewIndex (activeViews : View list byref) visitTile isTileBlocked =
    let topLeft = new Point(x, y + 1)
    let bottomRight = new Point(x + 1, y)
    let mutable viewIndex = viewIndex    
    while viewIndex < activeViews.Length && activeViews.[viewIndex].SteepLine.BelowOrColinear(bottomRight.X, bottomRight.Y) do
        viewIndex <- viewIndex + 1
    if not (viewIndex = activeViews.Length || activeViews.[viewIndex].ShallowLine.AboveOrColinear(topLeft.X, topLeft.Y)) then
        let mutable isBlocked = false
        let realX = x * dx
        let realY = y * dy
        if not <| List.exists ((=)(startX + realX, startY + realY)) visited then
            visited <- (startX + realX, startY + realY) :: visited
            visitTile(startX + realX, startY + realY)
        isBlocked <- isTileBlocked(startX + realX, startY + realY)

        if isBlocked then
            if activeViews.[viewIndex].ShallowLine.Above(bottomRight.X, bottomRight.Y) && activeViews.[viewIndex].SteepLine.Below(topLeft.X, topLeft.Y) then
                activeViews <- activeViews |> List.removeAt viewIndex
            elif activeViews.[viewIndex].ShallowLine.Above(bottomRight.X, bottomRight.Y) then
                addShallowBump topLeft.X topLeft.Y &activeViews viewIndex
                checkView &activeViews viewIndex |> ignore
            elif activeViews.[viewIndex].SteepLine.Below(topLeft.X, topLeft.Y) then
                addSteepBump bottomRight.X bottomRight.Y &activeViews viewIndex
                checkView &activeViews viewIndex |> ignore
            else 
                let shallowViewIndex = viewIndex
                viewIndex <- viewIndex + 1
                let mutable steepViewIndex = viewIndex
                // activeViews.insert(shallowViewIndex, copy.deepcopy(activeViews[shallowViewIndex]))
                activeViews <- List.insertAt shallowViewIndex ((activeViews.[shallowViewIndex] :> ICloneable).Clone() :?> View) activeViews

                addSteepBump bottomRight.X bottomRight.Y &activeViews shallowViewIndex

                if not <| checkView &activeViews shallowViewIndex then
                    viewIndex <- viewIndex - 1
                    steepViewIndex <- steepViewIndex - 1
                addShallowBump topLeft.X topLeft.Y &activeViews steepViewIndex
                checkView &activeViews steepViewIndex |> ignore

let private checkQuadrant (visited : (int * int) list byref) startX startY dx dy extentX extentY visitTile isTileBlocked = 
    let mutable activeViews = []
    let shallowLine = new Line(0, 1, extentX, 0)
    let steepLine = new Line(1, 0, 0, extentY)
    activeViews <- List.append activeViews [new View(shallowLine, steepLine)]
    let viewIndex = 0
    let maxI =  extentX + extentY
    let mutable i = 1
    while i <> maxI + 1 && activeViews.Length > 0 do
        let startJ = Math.Max(i - extentX, 0)
        let maxJ = Math.Min(i, extentY)
        let mutable j = startJ
        while j <> maxJ + 1 && viewIndex < activeViews.Length do
            let x = i - j
            let y = j 
            visitCoord &visited startX startY x y dx dy viewIndex &activeViews visitTile isTileBlocked
            j <- j + 1
        i <- i + 1

let visiblePositions (where : Point) radius board = 
    let mutable visited = [where.X, where.Y]        
    let result = ref []

    let minExtentX = Math.Min(where.X, radius)
    let maxExtentX = Math.Min(boardWidth - where.X - 1, radius)
    let minExtentY = Math.Min(where.Y, radius)
    let maxExtentY = Math.Min(boardHeight - where.Y - 1, radius)
    let visitTile = fun (x, y) -> 
        result := new Point(x, y) :: !result
    let isTileBlocked = fun (x, y) -> Board.isOpticalObstacle board (new Point(x, y))
    do 
        checkQuadrant &visited where.X where.Y 1 1 maxExtentX maxExtentY visitTile isTileBlocked
        checkQuadrant &visited where.X where.Y 1 -1 maxExtentX minExtentY visitTile isTileBlocked
        checkQuadrant &visited where.X where.Y -1 -1 minExtentX minExtentY visitTile isTileBlocked
        checkQuadrant &visited where.X where.Y -1 1 minExtentX maxExtentY visitTile isTileBlocked

    !result

let setVisibilityStates state  = 
    let playerPosition = getPlayerPosition state.Board
    let positions = visiblePositions playerPosition state.Player.SightRadius state.Board    
    let preResult = Array2D.mapi (fun x y place -> 
        let p = point x y
        if Seq.exists ((=) p) positions then {place with IsSeen = true; WasSeen = true} else {place with IsSeen = false}) state.Board.Places
    {state with Board = { state.Board with Places = preResult}}
