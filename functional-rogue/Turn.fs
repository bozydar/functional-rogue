module Turn

open System
open System.Drawing
open State
open Log
open Board
open Player
open Monsters

type private TurnAgentMessage = 
    | Elapse of decimal * State * AsyncReplyChannel<unit>
    | SubscribeStateChange of (State -> State)

let private turnAgent () =
    MailboxProcessor<TurnAgentMessage>.Start (fun inbox ->
    let rec loop time funcs = async {
        let! msg = inbox.Receive()            
        match msg with
        | SubscribeStateChange(func) ->                
             return! loop time (func :: funcs)
        | Elapse(elapsedTime, state, reply) -> 
            let elapsed = time + elapsedTime
            let iElapsed = Convert.ToInt32(Math.Floor(elapsed))
            let iState = Convert.ToInt32(Math.Floor(time))
            let turnsToGo = iElapsed - iState
            let numbers = seq { 0 .. (turnsToGo - 1)} 
            let evaluatedState = numbers |> Seq.fold (fun acc _ ->  acc |>> funcs) state
            State.set evaluatedState
            reply.Reply ()

            return! loop elapsed funcs
    }
    loop 0M []
)

let private agent = turnAgent () 

let elapse turns (state : option<State>) = 
    let myState = if state.IsSome then state.Value else State.get ()
    agent.PostAndReply(fun reply -> Elapse(turns, myState, reply))

let next state = 
    let turnsToGo = if state.Board.IsMainMap then 50M else 1M
    agent.PostAndReply(fun reply -> Elapse(turnsToGo, state, reply))

let subscribe stateChange = agent.Post(SubscribeStateChange(stateChange))

