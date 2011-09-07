module Broadcasting

open Log

//type private subscribers<_> = list< MailboxProcessor<_> >

type BroadcastingAgentMessage =
    | Subscribe of obj
    | Post of obj

let private broadcastingAgent () = 
    
    MailboxProcessor.Start (fun inbox ->
        let rec loop state = async {
            let! msg = inbox.Receive()            
            match msg with
            | Subscribe(value) ->                
                log Info (sprintf "Subscriber %s added" <| repr value)
                return! loop (value :: state)
            | Post(value) ->
                for item in state do   
                    let t = item.GetType() 
                    t.GetMethod("Post").Invoke(item, [|value|]) |> ignore
                return! loop state
        }
        loop []
    )

let agent<'T> = broadcastingAgent ()

let subscribe agentToSubscribe = agent.Post(Subscribe(agentToSubscribe))
let post message = agent.Post(Post(message))