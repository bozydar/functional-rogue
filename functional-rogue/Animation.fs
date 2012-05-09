module Animation

open Screen
open System.Drawing
open System

let moveSingleTextelAnimationFunction textel (boardFramePosition : Point) (moveFrom : Point) (moveTo : Point) =
    let animationFunction = (fun frameNr (currentScreen : textel[,]) ->
        let newx = (if moveFrom.X > moveTo.X then Math.Max(moveTo.X, moveFrom.X - frameNr) else Math.Min(moveFrom.X + frameNr, moveTo.X)) - boardFramePosition.X
        let newy = (if moveFrom.Y > moveTo.Y then Math.Max(moveTo.Y, moveFrom.Y - frameNr) else Math.Min(moveFrom.Y + frameNr, moveTo.Y)) - boardFramePosition.Y
        if newx = moveTo.X && newy = moveTo.Y then
            None
        else
            currentScreen.[newx,newy] <- textel
            Some(currentScreen)
        )
    animationFunction