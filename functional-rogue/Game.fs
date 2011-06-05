module Game

open System
open System.Drawing
open Log

let minRoomSize = 3

type Tile =
    | Wall 
    | Floor
    | Avatar
    | None

type Item = 
    | Sword
    | Vand

type CharacterType = 
    | Avatar
    | Monster
    | NPC

type Character = {
    Type: CharacterType
}    

type Place = {
    Tile: Tile; 
    Items: Item list;
    Character: Character option;    
} with
    static member EmptyPlace = 
            {Tile = Tile.None; Items = []; Character = Option.None }

module Board =

    let boardHeight = 24
    let boardWidth = 79

    type Board = Place[,]
                    
    let get (board: Board) (point: Point) = Array2D.get board point.X point.Y

    let set (point: Point) (value: Place) (board: Board) : Board =
        let result = Array2D.copy board 
        Array2D.set result point.X point.Y value
        result

    let places (board: Board) = 
        seq {
            for x = 0 to boardWidth - 1 do
                for y = 0 to boardHeight - 1 do
                    let item = Array2D.get board x y
                    yield (new Point(x, y), item)
        }

    let moveCharacter (character: Character) (newPosition: Point) (board: Board) =
        match Seq.tryFind (fun (point, place) -> 
            match place.Character with 
            | Some(character1) -> character1 = character
            | _ -> false) (places board) with        
        | Some((oldPosition, oldPlace)) ->             
            let newPlace = { oldPlace with Character = (get board oldPosition ).Character }
            board 
            |> set newPosition newPlace 
            |> set oldPosition { oldPlace with Character = Option.None }
        | _ ->             
            let oldPlace = get board newPosition
            let newPlace = { oldPlace with Character = Some character }
            board
            |> set newPosition newPlace

    let emptyBoard : Board = Array2D.create boardWidth boardHeight Place.EmptyPlace
        
open Board


type IModifier =
    abstract member Modify: Board -> Board

type Room(rect: Rectangle) =
    let generateRoom width height = 
        if width < 1 then invalidArg "width" "Is zero"
        if height < 1 then invalidArg "height" "Is zero"

        let wall = {Place.EmptyPlace with Tile = Tile.Wall}
        let floor = {Place.EmptyPlace with Tile = Tile.Floor}
        let result = Array2D.create width height floor
        for x = 0 to width - 1 do 
            Array2D.set result x 0 wall
            Array2D.set result x (height - 1) wall
        for y = 0 to height - 1 do 
            Array2D.set result 0 y wall
            Array2D.set result (width - 1) y wall
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
        let height = boardHeight - 1
        let width = boardWidth - 1
        for x = 0 to width do
            for y = 0 to height do        
                let newItem = newBoard.[x, y]
                if not (oldBoard.[x, y] = newItem) then 
                    yield (x, y, newItem)
    }

    let printItem = fun item -> 
        let char =             
            match item.Character with
            | Some(character1) -> 
                match character1.Type with
                | Avatar -> "@"
                | Monster -> "s"
                | NPC -> "P"
            | _ -> 
                match item.Items with
                | h::_ -> "i"
                | _ -> 
                    match item.Tile with
                    | Wall ->  "#"
                    | Floor -> "."
                    | _ -> " "
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
    let mutable board = Array2D.create boardWidth boardHeight {Place.EmptyPlace with Tile = Tile.Floor}
    let rooms = (generateRooms []) |> Seq.take 4 |> Seq.toList
    for item in rooms do        
        board <- (item :> IModifier).Modify board 
    board

type Command = 
    | Up
    | Down
    | Left
    | Right
    | Wait
    | Quit
    | Unknown

let commandToSize command = 
    match command with
    | Up -> new Size(0, -1)
    | Down -> new Size(0, 1)
    | Left -> new Size(-1, 0)
    | Right -> new Size(1, 0)
    | _ -> new Size(0, 0)

let moveCharacter character command board = 
    let position, _ =  Seq.find (fun (_, place) -> place.Character = Some character) <| places board

    let move = commandToSize command
    let newPosition = position + move
    let newPlace = get board newPosition
    
    match newPlace.Tile with 
    | Wall -> board
    | _ ->         
        board |> moveCharacter character newPosition

type State = {
    Board: Board
}


let mainLoop() =
    let rec loop printAll (states: State list) =                
        let nextTurn command = 
            let last = states.Head
            // Something... Something...
            //let avatar = last.Avatar.Move command
            let board = 
                last.Board  
                |> moveCharacter {Type = Avatar} command
            {Board = board}

        states.Head.Board 
        |> if printAll then printBoard Board.emptyBoard else printBoard states.[1].Board

        let char = System.Console.ReadKey(true)        
        
        let command = 
            match char.Key with 
            | ConsoleKey.UpArrow -> Up            
            | ConsoleKey.DownArrow -> Down            
            | ConsoleKey.LeftArrow -> Left            
            | ConsoleKey.RightArrow -> Right
            | ConsoleKey.W -> Wait
            | ConsoleKey.Escape -> Quit
            | _ -> Unknown                        
        
        match command with
        | Quit -> ()
        | Unknown -> loop false states
        | Up | Down | Left | Right | Wait -> loop false ((nextTurn command) :: states)

    let board = 
        generateLevel 
        |> Board.moveCharacter {Type = CharacterType.Avatar} (new Point(1, 1))

    let entryState = {         
        Board = board; 
    }
    loop true [entryState]

        
[<EntryPoint>]
let main args =    
    mainLoop()
    0
