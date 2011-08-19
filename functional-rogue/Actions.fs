module Actions

open System
open System.Drawing
open State
open Log
open Board
open LevelGeneration
open Screen
open Sight
open Items
open Player

type Command = 
    | Up
    | Down
    | Left
    | Right
    | UpLeft
    | UpRight
    | DownLeft
    | DownRight
    | Wait
    | Take
    | ShowItems
    | Quit
    | Unknown
    | OpenDoor
    | CloseDoor
    | ShowEquipment
    | ShowMessages
    | Harvest


let private commandToSize command = 
    match command with
    | Up -> new Size(0, -1)
    | Down -> new Size(0, 1)
    | Left -> new Size(-1, 0)
    | Right -> new Size(1, 0)
    | UpLeft -> new Size(-1, -1)
    | UpRight -> new Size(1, -1)
    | DownLeft -> new Size(-1, 1)
    | DownRight -> new Size(1, 1)
    | _ -> new Size(0, 0)

let moveCharacter command state = 
    let board = state.Board
    let playerPosition = getPlayerPosition board

    let move = commandToSize command
    let newPosition = playerPosition + move
    let newPlace = get board newPosition
    
    let preResult =
        if (isObstacle board newPosition) then
            board
        else
            board |> moveCharacter {Type = Avatar; Monster = Option.None} newPosition
    {state with Board = preResult}

let operateDoor command board =
    let playerPosition = getPlayerPosition board
    let oldDoor = {Place.EmptyPlace with Tile = (if (command = OpenDoor) then Tile.ClosedDoor else Tile.OpenDoor)}
    let newDoor = {Place.EmptyPlace with Tile = (if (command = OpenDoor) then Tile.OpenDoor else Tile.ClosedDoor)}
    for x in (max 0 (playerPosition.X - 1))..(min boardWidth (playerPosition.X + 1)) do
        for y in (max 0 (playerPosition.Y - 1))..(min boardHeight (playerPosition.Y + 1)) do
            if(not(x = playerPosition.X && y = playerPosition.Y) && board.[x,y].Tile = oldDoor.Tile) then
                Array2D.set board x y newDoor
    board 

let performCloseOpenAction command state =
    match command with
    | OpenDoor | CloseDoor -> { state with Board = operateDoor command state.Board }
    | _ -> state

let performTakeAction command state =     
    if command = Take then
        let playerPosition = getPlayerPosition state.Board
        let place = get state.Board playerPosition
        let takenItems = place.Items
        let pickUpMessages = List.map (fun i -> (sprintf "You have picked up an item: %s" (itemShortDescription i))) takenItems
        let board1 = 
            state.Board
            |> set playerPosition {place with Items = []}
        let state1 = 
            let shortCuts = createShortCuts state.Player.ShortCuts takenItems
            
            { state with Player = { state.Player with Items =  takenItems @ state.Player.Items; ShortCuts = shortCuts }}
            |> addMessages pickUpMessages

        {state1 with Board = board1}
    else
        state

let performHarvest command state = 
    if command = Harvest then
        let playerPosition = getPlayerPosition state.Board
        let place = get state.Board playerPosition
        let takenOre = place.Ore
        let player1 = 
            match takenOre with
            | Iron(quantity) -> {state.Player with Iron = state.Player.Iron + quantity}
            | Gold(quantity) -> {state.Player with Gold = state.Player.Gold + quantity}
            | Uranium(quantity) -> {state.Player with Uranium = state.Player.Uranium + quantity}
            | _ -> state.Player
        match takenOre with
            | Iron(quantity) | Gold(quantity) | Uranium(quantity) ->
                let pickUpMessage = sprintf "You have harvested ore %s. Quantity: %i" (repr takenOre) quantity
                let board1 = 
                    state.Board
                    |> set playerPosition {place with Ore = NoneOre}
                {state with Board = board1; Player = player1} |> addMessage pickUpMessage                
            | NoneOre -> 
                state
    else
        state