module Items

open System
open Characters

[<CustomEquality; CustomComparison>]
type Item = {    
    Id : Guid;
    Name : string;
    Wearing : Wearing
    Offence : Factor;
    Defence : Factor;
    Type : Type;
    MiscProperties : MiscProperties;
    Attack : (Character -> Character -> damage) option
} 
with 
    override this.Equals(other) =
        match other with
        | :? Item as other -> other.Id = this.Id
        | _ -> false
    
    override this.GetHashCode() = 
        hash this.Id

    interface IComparable with
        member this.CompareTo(other) =
            match other with
            | :? Item as other -> compare this other
            | _ -> invalidArg "other" "cannot compare values of different types"        
and Wearing = {
    OnHead : bool;
    InHand : bool;
    OnTorso : bool;
    OnLegs : bool;
} 
with
    static member NotWearable = { OnHead = false; InHand = false; OnTorso = false; OnLegs = false }
    static member HeadOnly = { Wearing.NotWearable with OnHead = true }
    static member HandOnly = { Wearing.NotWearable with InHand = true }
    static member TorsoOnly = { Wearing.NotWearable with OnTorso = true }
    static member LegsOnly = { Wearing.NotWearable with OnLegs = true }
and Type = 
    | Stick
    | Rock
    | Sword
    | Hat
    | Corpse
    | Tool
and MiscProperties = {
    OreExtractionRate : int
}

let defaultMiscProperties = {
    OreExtractionRate = 0
}

let itemShortDescription item =
    let rest = 
        item.Name + " - "
        + if not item.Offence.IsZero then sprintf "Offence: %s " (item.Offence.ToString()) else ""  
        + if not item.Defence.IsZero  then sprintf "Defence: %s " (item.Defence.ToString()) else ""
    rest
