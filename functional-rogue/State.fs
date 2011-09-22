module State

open Board
open Player
open System.Drawing
open Monsters
open Characters
open System
open System.IO
open System.Runtime.Serialization.Formatters.Binary

type State = {
    Board: Board;
    BoardFramePosition: Point;
    Player: Player;
    TurnNumber: int;
    UserMessages: (int*string) list;
    AllBoards : System.Collections.Generic.Dictionary<System.Guid,Board>;
    MainMapGuid : System.Guid;
    AvailableReplicationRecipes : System.Collections.Generic.HashSet<string>
} 

type private StateAgentMessage = 
    | Set of AsyncReplyChannel<unit> * Option<State>
    | Get of AsyncReplyChannel<Option<State>>

let private stateAgent () = 
    
    MailboxProcessor<StateAgentMessage>.Start (fun inbox ->
        let rec loop state = async {
            let! msg = inbox.Receive()            
            match msg with
            | Set(replyChannel, value) -> 
                replyChannel.Reply ()   
                return! loop value
            | Get(replyChannel) -> 
                replyChannel.Reply(state)   
                return! loop (state)
        }
        loop Option.None 
    )

let private agent = stateAgent () 

let set (state: State) = 
    agent.PostAndReply(fun replyChannel -> Set(replyChannel, Some(state)))

let get () = 
    match agent.PostAndReply(fun replyChannel -> Get(replyChannel)) with
    | Some(value) -> value
    | Option.None -> failwith "Cannot return None"

let addMessages (messages : string list) (state : State) =
    let newMessages = List.map (fun m -> (state.TurnNumber, m)) messages
    { state with UserMessages = (List.append newMessages state.UserMessages) }

let addMessage (message : string) (state : State) =
    addMessages [message] state

let private filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\functional-rogue.save";

let writeState (state : State) =
    let outputStream = File.Create(filePath)
    let formatter = new BinaryFormatter()
    formatter.Serialize(outputStream, state)

let loadState () : State =
    let inputSream = File.OpenRead(filePath)
    let formatter = new BinaryFormatter()
    formatter.Deserialize(inputSream) :?> State
