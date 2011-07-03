module Characters

(*
open Board

type Command = 
    | Up
    | Down
    | Left
    | Right
    | Wait
    | Quit
    | Unknown


type CharacterMessage = {
    Command: Commad
}

let private charactersAgent () = 
    
    MailboxProcessor<CharacterMessage>.Start (fun inbox ->
        let rec loop avatar = async {
            let! msg = inbox.Receive()
            match msg with
            | Move
            let newAvatar = 
                
        }
    )
    *)