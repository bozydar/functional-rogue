module Items

//type ItemProperties = {
//    Name: string
//    Weight: decimal;
//    Description: string
//}

//type Item = 
//    | Plain of int * ItemProperties // id and properties
//    | Gold of int
//
//let all = 
//    List.mapi (fun i item -> Plain(i + 1, item)) 
//    <| [
//        {Name = "Wooden staff"; Weight = 1.5M; Description = "Simple wooden staff"}
//        {Name = "Screwdriver"; Weight = 0.1M; Description = "Steel screwdriver"}
//    ]

type Item = {
    Id : int
    Name : string
    Class : ItemClass
} 
    and ItemClass = 
    | Stick of Stick
    | Sword of Sword
    | Hat of Hat
and 
    Stick = {
    Damage : int
    } 
and Sword = {
    Damage : int
    }
and Hat = {
    Defense : int
    }

let itemShortDescription item =
    match item.Class with
    | Stick x -> sprintf "Stick - damage: %i" x.Damage
    | Sword x -> sprintf "Sword - damage: %i" x.Damage
    | Hat x -> sprintf "Hat - defense: %i" x.Defense

let canHoldItemInHand item =
    match item.Class with
    | Stick x -> true
    | Sword x -> true
    | _ -> false