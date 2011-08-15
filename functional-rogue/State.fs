﻿module State

open Board
open Items
open Player
open System.Drawing
open Monsters

type State = {
    Board: Board;
    BoardFramePosition: Point;
    Player: Player;
    TurnNumber: int;
    UserMessages: (int*string) list
    Monsters: Monster list
} 

type private StateAgentMessage = 
    | Set of Option<State>
    | Get of AsyncReplyChannel<Option<State>>

let private stateAgent () = 
    
    MailboxProcessor<StateAgentMessage>.Start (fun inbox ->
        let rec loop state = async {
            let! msg = inbox.Receive()            
            match msg with
            | Set(value) -> return! loop value
            | Get(replyChannel) -> 
                    replyChannel.Reply(state)   
                    return! loop (state)
        }
        loop Option.None 
    )

let private agent = stateAgent () 

let set (state: State) = agent.Post(Set(Some(state)))

let get () = 
    match agent.PostAndReply(fun replyChannel -> Get(replyChannel)) with
    | Some(value) -> value
    | Option.None -> failwith "Cannot return None"

let addMessages (messages : string list) (state : State) =
    let newMessages = List.map (fun m -> (state.TurnNumber,m)) messages
    { state with UserMessages = (List.append newMessages state.UserMessages) }