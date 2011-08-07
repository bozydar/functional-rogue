module State

open Board
open Items
open System.Drawing


type State = {
    Board: Board;
    BoardFramePosition: Point;
    Player: Player;
    TurnNumber: int;
} and Player = {
    Name: string;
    HP: int;  // life
    MaxHP: int;
    Magic: int;  // magic
    MaxMagic: int;
    Gold: int // gold
    SightRadius: int // sigh radius
    Items: list<Item>
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
