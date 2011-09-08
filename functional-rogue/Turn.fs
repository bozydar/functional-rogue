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
    | Elapse of decimal * State * AsyncReplyChannel<unit>


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
    let rec loop time = async {
        let! msg = inbox.Receive()            
        match msg with
        | Elapse(elapsedTime, state, reply) -> 
            let elapsed = time + elapsedTime
            let iElapsed = Convert.ToInt32(Math.Floor(elapsed))
            let iState = Convert.ToInt32(Math.Floor(time))
            let turnsToGo = iElapsed - iState
            if turnsToGo > 0 then 
                for i = 1 to turnsToGo do
                    let state1 = 
                        state
                        |> handleMonsters
                        |> setVisibilityStates
                        |> evaluateBoardFramePosition                                    
                    State.set {state1 with TurnNumber = state.TurnNumber + 1}
            reply.Reply ()

            return! loop elapsed
    }
    loop 0M
)

let private agent = turnAgent () 

let elapse turns (state : option<State>) = 
    let myState = if state.IsSome then state.Value else State.get ()
    agent.PostAndReply(fun reply -> Elapse(turns, myState, reply))

let next state = agent.PostAndReply(fun reply -> Elapse(1M, state, reply))

