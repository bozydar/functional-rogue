module Game

open System
open System.Drawing
open Log

let boardHeight = 24
let boardWidth = 79
let minRoomSize = 3

type Tile =
    | Wall 
    | Floor
    | Avatar
    | None

type Board = Tile[,]

let (@) (board: Board) (point: Point) =
    Array2D.get board point.X point.Y

type IModifier =
    abstract member Modify: Board -> Board

type Room(rect: Rectangle) =
    let generateRoom width height = 
        if width < 1 then invalidArg "width" "Is zero"
        if height < 1 then invalidArg "height" "Is zero"

        let result = Array2D.create width height Tile.Floor
        for x = 0 to width - 1 do 
            Array2D.set result x 0 Tile.Wall
            Array2D.set result x (height - 1) Tile.Wall
        for y = 0 to height - 1 do 
            Array2D.set result 0 y Tile.Wall
            Array2D.set result (width - 1) y Tile.Wall
        result

    member this.Rectangle
        with get() = rect

    override this.ToString() =
        rect.ToString()
    interface IModifier with
        member this.Modify board = 
            let room = generateRoom rect.Width rect.Height
            let width = Array2D.length1 room
            let height = Array2D.length2 room
            let result = board
            Array2D.blit room 0 0 result rect.X rect.Y width height
            result
            
let printBoard (oldBoard: Board) (newBoard: Board) = 
    
    let compareBoard() = seq {
        let height = Array2D.length1 oldBoard
        let width = Array2D.length2 oldBoard         
        for y = 0 to width - 1 do
            for x = 0 to height - 1 do        
                let newItem = newBoard.[x, y]
                if not (oldBoard.[x, y] = newItem) then 
                    yield (x, y, newItem)
    }
    let printItem = fun item -> 
        let char = 
            match item with
            | Wall ->  "#"
            | Floor -> "."
            | Avatar -> "@"
            | None -> " "
        Console.Write char
    for x, y, item in compareBoard() do        
        Console.SetCursorPosition(x, y)
        printItem item

let r = new Random()
let rnd max = 
    r.Next(max)

let rnd2 min max = 
    r.Next(min, max)
   
let isOverlapping room rooms =    

    let isOverlapping2 (room1: Room) (room2: Room) =         
        room1.Rectangle.IntersectsWith room2.Rectangle

    List.exists (isOverlapping2 room) rooms

let rec generateRooms rooms =
    seq {                        
        let rec newRoom'() = 
            let newRoom = 
                let posX = rnd (boardWidth - minRoomSize)
                let posY = rnd (boardHeight - minRoomSize)
                let width = rnd2 minRoomSize (boardWidth - posX)
                let height = rnd2 minRoomSize (boardHeight - posY)
                new Room(new Rectangle(posX, posY, width, height))
            if isOverlapping newRoom rooms 
            then
                newRoom'()
            else
                newRoom
        let newRoom = newRoom'()
        yield newRoom
        yield! generateRooms (newRoom::rooms)
    }

let generateLevel: Board = 
    let mutable board = Array2D.create boardWidth boardHeight Tile.Floor            
    let rooms = (generateRooms []) |> Seq.take 4 |> Seq.toList
    for item in rooms do        
        board <- (item :> IModifier).Modify board 
    board

type Command = 
    | Up
    | Down
    | Left
    | Right
    | Quit
    | Unknown

let commandToSize command = 
    match command with
    | Up -> new Size(0, -1)
    | Down -> new Size(0, 1)
    | Left -> new Size(-1, 0)
    | Right -> new Size(1, 0)
    | _ -> invalidArg "command" ("bad command " + (repr command))

type Avatar(board: Board, position: Point, ?previous: Avatar) = 
    let putOnTile = board @ position
    interface IModifier with
        member this.Modify board =            
            Array2D.set board position.X position.Y Tile.Avatar
            if previous.IsSome then Array2D.set board previous.Value.Position.X previous.Value.Position.Y putOnTile            
            board

    member this.Move command =
        let size = commandToSize command            
        new Avatar(board, position + size, this)
    
    member this.Position 
        with get() = position

type State = {
    OldBoard: Board; Board: Board; Avatar: Avatar
}

let mainLoop() =
    let rec loop printAll (state: State) =                
        let board = (state.Board.Clone() :?> Board) |> (state.Avatar :> IModifier).Modify 
        // problem is that new state is bitored by new state and old avatar is not cleared
        // consider using events for each of state elements to trace all changes and indicate them on board
        board |> if printAll then printBoard (Array2D.create boardWidth boardHeight Tile.None) else printBoard state.Board 

        let char = System.Console.ReadKey(true)        
        
        let command = 
            match char.Key with 
            | ConsoleKey.UpArrow -> Up            
            | ConsoleKey.DownArrow -> Down            
            | ConsoleKey.LeftArrow -> Left            
            | ConsoleKey.RightArrow -> Right
            | ConsoleKey.Escape -> Quit
            | _ -> Unknown                        
        
        match command with
        | Quit -> ()
        | Unknown -> loop false state
        | _ ->    
            match board @ (state.Avatar.Position + commandToSize command) with
            | Wall -> loop false state
            | _ -> 
                let newState = { state with OldBoard = state.Board; Board = board; Avatar = state.Avatar.Move command }
                loop false newState

    let board = generateLevel
    let entryState = { 
        OldBoard = Array2D.create boardWidth boardHeight Tile.None;
        Board = board; 
        Avatar = new Avatar(board, new Point(1, 1)) 
    }
    loop true entryState

        
[<EntryPoint>]
let main args =    
    mainLoop()
    0
