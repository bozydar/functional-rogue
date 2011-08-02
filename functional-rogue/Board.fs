module Board

open System
open System.Drawing

type Tile =
    | Wall 
    | Floor
    | Avatar
    | None

type Item = 
    | Sword
    | Vand
    | Gold of int

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
    IsSeen: bool;
    WasSeen: bool;
} with
    static member EmptyPlace = 
            {Tile = Tile.None; Items = []; Character = Option.None; IsSeen = false; WasSeen = false }
    static member Wall = 
            {Tile = Tile.Wall; Items = []; Character = Option.None; IsSeen = false; WasSeen = false}

let boardHeight = 24
let boardWidth = 79

type Board = Place[,] 
    
    
let boardContains (point: Point) = 
    boardWidth > point.X  && boardHeight > point.Y && point.X >= 0 && point.Y >= 0
                    
let get (board: Board) (point: Point) = if boardContains point then Array2D.get board point.X point.Y else Place.Wall

let isObstacle (board: Board) (point: Point) = (get board point).Tile = Tile.Wall

let set (point: Point) (value: Place) (board: Board) : Board =
    let result = Array2D.copy board 
    Array2D.set result point.X point.Y value
    result

let modify (point: Point) (modifier: Place -> Place) (board: Board) =
    let current = get board point 
    set point (modifier current) board

let places (board: Board) = 
    seq {
        for x = 0 to boardWidth - 1 do
            for y = 0 to boardHeight - 1 do
                let item = Array2D.get board x y
                yield (new Point(x, y), item)
    }

let getPlayerPosition (board: Board) = 
    let preResult = Seq.tryFind (fun (point, place) -> 
        match place.Character with 
        | Some(character1) -> character1 = {Type = Avatar}
        | _ -> false) (places board)
    let point, _ = preResult.Value
    point

let moveCharacter (character: Character) (newPosition: Point) (board: Board) =
    match Seq.tryFind (fun (point, place) -> 
        match place.Character with 
        | Some(character1) -> character1 = character
        | _ -> false) (places board) with        
    | Some((oldPosition, oldPlace)) when oldPosition <> newPosition ->             
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


type private BoardMessage = 
| GetAt of Point * AsyncReplyChannel<Place>
| SetAt of Point * Place
| Apply of (Board -> Board)
| Set of Board

let private createProcessor board =
    MailboxProcessor<BoardMessage>.Start(fun inbox ->
        let rec loop (board: Board) = async {
            let! msg = inbox.Receive()
            match msg with 
            | GetAt(point, outbox) -> 
                outbox.Reply(Array2D.get board point.X point.Y)
                return! loop board
            | SetAt(point, place) ->
                board.[point.X, point.Y] <- place
                return! loop board
            | Apply(func) ->
                return! loop (func board)
            | Set(board) ->
                return! loop board
        }
        loop board)
        
let private agent = createProcessor <| Array2D.create boardWidth boardHeight Place.EmptyPlace

// TODO: refact functions to use BoardMessage structure

