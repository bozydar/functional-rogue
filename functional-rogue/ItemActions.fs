module ItemActions

open Characters
open State
open Predefined.Items
open System

let performUseItemDrone  (item : Item) (state : State)=
    match item.Name with
    | name when name = reconnaissanceDrone.Name ->
        if state.Board.Level < 0 then
            state |> addMessage (sprintf "You cannot use this item under ground.")
        else
            let sightRadiusMultiplier = 5
            let reconDroneModifier = {
                Type = PlayerSightMultiplier(sightRadiusMultiplier);
                TurnOnOnTurnNr = 0;
                TurnOffOnTurnNr = 0;
                OnTurningOn = (fun state ->
                    state.Player.SightRadiusMultiplier <- state.Player.SightRadiusMultiplier + sightRadiusMultiplier
                    state.Player.CanSeeThroughWalls <- true
                    state |> addMessage (sprintf "The Reconnaissance Drone flies up into the air sending the video recording of the nearby surroundings into your pocket computer.")
                    )
                OnEachTurn = (fun state -> state)
                OnTurnigOff = (fun state ->
                    state.Player.SightRadiusMultiplier <- Math.Min(1, state.Player.SightRadiusMultiplier - sightRadiusMultiplier)
                    state.Player.CanSeeThroughWalls <- false
                    state |> addMessage (sprintf "The Reconnaissance Drone falls down ending its transmission.")
                    )
                }
            state |> addTemporaryModifier reconDroneModifier 0 2
    | _ -> state

let performUseInjector (item : Item) (state : State)=
    state |> addMessage (sprintf "The injector does not work.")

let getUseItemFunction (item : Item) =
    match item.Type with
    | Drone -> Some(performUseItemDrone)
    | Injector -> Some(performUseInjector)
    | _ -> None

let performUseItemAction (item : Item) (state : State)=
    let useItemFunction = getUseItemFunction item
    if useItemFunction.IsSome then
        useItemFunction.Value item state
    else
        state