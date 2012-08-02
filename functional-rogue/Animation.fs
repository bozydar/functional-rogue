module Animation

open Screen
open System.Drawing
open System

let moveSingleTextelAnimationFunction textel (boardFramePosition : Point) (moveFrom : Point) (moveTo : Point) =
    let animationFunction = (fun frameNr (currentScreen : textel[,]) ->
        let currentPoint = (getBresenhamLinePoints moveFrom moveTo) |> Seq.nth frameNr
        if currentPoint = moveTo then
            None
        else
            let newX = currentPoint.X - boardFramePosition.X
            let newY = currentPoint.Y - boardFramePosition.Y
            currentScreen.[newX,newY] <- textel
            Some(currentScreen)
        )
    animationFunction