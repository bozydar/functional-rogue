﻿module Items

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
    | Corpse

let itemShortDescription item =
    let rest = 
        item.Name + " - "
        + if not item.Offence.IsZero then sprintf "Offence: %s " (item.Offence.ToString()) else ""  
        + if not item.Defence.IsZero  then sprintf "Defence: %s " (item.Defence.ToString()) else ""
    rest
    