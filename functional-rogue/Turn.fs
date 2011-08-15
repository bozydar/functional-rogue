module Turn

open System
open System.Drawing
open State
open Log
open Board
open LevelGeneration
open Screen
open Sight
open Items
open Actions
open Player
open Monsters
open AI

type private TurnAgentMessage = 
    | Elapse of decimal * Command * AsyncReplyChannel<unit>


let private evaluateBoardFramePosition state = 
    let playerPosition = getPlayerPosition state.Board
    let frameView = new Rectangle(state.BoardFramePosition, boardFrameSize)
    let preResult =
        let x = inBoundary (playerPosition.X - (boardFrameSize.Width / 2)) 0 (boardWidth - boardFrameSize.Width)
        let y = inBoundary (playerPosition.Y - (boardFrameSize.Height / 2)) 0 (boardHeight - boardFrameSize.Height)
        point x y                
    { state with BoardFramePosition = preResult }

let private turnAgent () =
    MailboxProcessor<TurnAgentMessage>.Start (fun inbox ->
    let rec loop state = async {
        let! msg = inbox.Receive()            
        match msg with
        | Elapse(elapsedTime, command, reply) -> 
            let elapsed = state + elapsedTime
            let iElapsed = Convert.ToInt32(Math.Floor(elapsed))
            let iState = Convert.ToInt32(Math.Floor(state))
            let turnsToGo = iElapsed - iState
            if turnsToGo > 0 then 
                for i = 1 to turnsToGo do
                    let state = 
                        State.get ()
                        |> moveCharacter command
                        |> handleMonsters
                        |> performCloseOpenAction command
                        |> performTakeAction command
                        |> setVisibilityStates
                        |> evaluateBoardFramePosition                                    
                    State.set {state with TurnNumber = state.TurnNumber + 1}
            reply.Reply ()

            return! loop elapsed
    }
    loop 0M
)

let private agent = turnAgent () 

let elapse turns command = agent.PostAndReply(fun reply -> Elapse(turns, command, reply))
let next command = agent.PostAndReply(fun reply -> Elapse(1M, command, reply))

