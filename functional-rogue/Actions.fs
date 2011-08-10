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

type Command = 
    | Up
    | Down
    | Left
    | Right
    | Wait
    | Take
    | ShowItems
    | Quit
    | Unknown
    | OpenDoor
    | CloseDoor
    | ShowEquipment


let private commandToSize command = 
    match command with
    | Up -> new Size(0, -1)
    | Down -> new Size(0, 1)
    | Left -> new Size(-1, 0)
    | Right -> new Size(1, 0)
    | _ -> new Size(0, 0)

let moveCharacter command state = 
    let board = state.Board
    let playerPosition = getPlayerPosition board

    let move = commandToSize command
    let newPosition = playerPosition + move
    let newPlace = get board newPosition
    
    let preResult = 
        match newPlace.Tile with 
        | Wall | ClosedDoor | Tree -> board
        | _ ->         
            board |> moveCharacter {Type = Avatar} newPosition
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
    let playerPosition = getPlayerPosition state.Board
    if command = Take then
        let place = get state.Board playerPosition
        let takenItems = place.Items
        let board1 = 
            state.Board
            |> set playerPosition {place with Items = []}
        let state1 = 
            let extractGold items = 0
                //Seq.sumBy (function | Gold(value) -> value | _ -> 0) items
            let gold = state.Player.Gold + extractGold takenItems
            { state with Player = { state.Player with Items =  takenItems @ state.Player.Items; Gold = gold}}

        {state1 with Board = board1}
    else
        state
