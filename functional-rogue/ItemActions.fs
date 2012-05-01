module ItemActions

open Characters
open State
open Predefined.Items
open System
open Screen

let performUseItemDrone  (item : Item) (state : State)=
    match item.Name with
    | name when name = createReconnaissanceDrone().Name ->
        if state.Board.Level < 0 then
            state |> addMessage (sprintf "You cannot use this item under ground.")
        else
            let sightRadiusMultiplier = 5
            let reconDroneModifier = {
                Type = PlayerSightMultiplier(sightRadiusMultiplier);
                TurnOnOnTurnNr = 0;
                TurnOffOnTurnNr = 0;
                StateChangeFunction = (fun relativeTurnNr relativeLastTurnNr state ->
                    match relativeTurnNr with
                    | 0 ->
                        state.Player.SightRadiusMultiplier <- state.Player.SightRadiusMultiplier + sightRadiusMultiplier
                        state.Player.CanSeeThroughWalls <- true
                        state |> addMessage (sprintf "The Reconnaissance Drone flies up into the air sending the video recording of the nearby surroundings into your pocket computer.")
                    | var when var = relativeLastTurnNr ->
                        state.Player.SightRadiusMultiplier <- Math.Min(1, state.Player.SightRadiusMultiplier - sightRadiusMultiplier)
                        state.Player.CanSeeThroughWalls <- false
                        state |> addMessage (sprintf "The Reconnaissance Drone falls down ending its transmission.")
                    | _ -> state
                )
                }
            state |> addTemporaryModifier reconDroneModifier 0 2
    | _ -> state

let performUseInjector (item : Item) (state : State)=
    let injectionEffectFunction (liquidType : LiquidType) : State -> State =
        (fun state ->
            match liquidType with
            | HealingSolution ->
               state.Player.CurrentHP <- Math.Min(state.Player.CurrentHP + 1, state.Player.MaxHP)
               state
            | _ ->state
        )

    if not item.IsLiquidContainer then
        failwith "Wrong argument"
    else
        if item.Container.Value.LiquidInside.IsNone then
            state |> addMessage (sprintf "The injector is empty.")
        else
            let injectionAmountOptions = [10.0<ml>..10.0<ml>..item.Container.Value.LiquidInside.Value.Amount]
            let chosenAmount = chooseListItemThroughPagedDialog "How much do you want to inject?" (fun i -> i.ToString()) injectionAmountOptions
            if chosenAmount.IsSome then
                let injectionModifier = {
                    Type = Default;
                    TurnOnOnTurnNr = 0;
                    TurnOffOnTurnNr = 0;
                    StateChangeFunction = (fun relativeTurnNr relativeLastTurnNr state ->
                        match relativeTurnNr with
                        | 0 ->
                            state
                            |> addMessage (sprintf "You inject " + chosenAmount.Value.ToString() + "ml of " + getUnionCaseName item.Container.Value.LiquidInside.Value.Type + " into your body.")
                            |> injectionEffectFunction item.Container.Value.LiquidInside.Value.Type
                        | var when var = relativeLastTurnNr ->
                            state
                            |> addMessage (sprintf "The effect of " + getUnionCaseName item.Container.Value.LiquidInside.Value.Type + " injection wears off.")
                            |> injectionEffectFunction item.Container.Value.LiquidInside.Value.Type
                        | _ ->
                            state |> injectionEffectFunction item.Container.Value.LiquidInside.Value.Type
                    )
                }
                ignore (item.TakeLiquid chosenAmount.Value)
                state |> addTemporaryModifier injectionModifier 0 (int(chosenAmount.Value) / 5)
            else
                state

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