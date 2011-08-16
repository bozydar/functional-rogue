module Items

type Item = {
    Id : int;
    Name : string;
    Wearing : Wearing
    Offence : Factor;
    Defence : Factor;
    Type : Type;
} 
and Wearing = {
    OnHead : bool;
    InHand : bool;
    OnTorso : bool;
    OnLegs : bool;
} 
and Type = 
    | Stick
    | Sword
    | Hat

let itemShortDescription item =
    let type' = repr item.Type
    let rest = 
        if not item.Offence.IsZero then sprintf "Offence: %s " (item.Offence.ToString()) else ""  
        + if not item.Defence.IsZero  then sprintf "Defence: %s " (item.Defence.ToString()) else ""
    rest
    