module ItemActions

open Characters
open State
open Predefined.Items

let performUseItemDrone  (item : Item) (state : State)=
    match item.Name with
    | name when name = reconnaissanceDrone.Name ->
        if state.Board.Level < 0 then
            state |> addMessage (sprintf "You cannot use this item under ground.")
        else
            let sightRadiusMultiplier = 5
            let endEffectTurn = state.TurnNumber + 3
            state.Player.SightRadiusMultiplier <- state.Player.SightRadiusMultiplier + sightRadiusMultiplier
            let modifiers =
                [{ Type = PlayerSightMultiplier(5); TurnOffOnTurnNr = endEffectTurn }]
                @[ { Type = SeeThroughWalls(true); TurnOffOnTurnNr = endEffectTurn }]
            { state with TemporaryModifiers = state.TemporaryModifiers @ modifiers }
    | _ -> state

let performUseItemAction (item : Item) (state : State)=
    match item.Type with
    | Drone -> performUseItemDrone item state
    | _ -> state