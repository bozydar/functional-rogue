module Player

open System
open System.Collections.Generic
open Items
open Characters
open Quantity

type WornItems = {
    Head : option<Item>;
    Hand : option<Item>;    
    Torso : option<Item>;
    Legs : option<Item>
} 

type Player (name : string, hp : int) = 
    inherit Character (CharacterType.Avatar)

    let mutable hP = hp

    let maxHp = hp

    let mutable iron : int = 0

    let mutable gold : int = 0

    let mutable uranium : int = 0

    let mutable water : int = 0

    let mutable contaminatedWater : int = 0

    let mutable items : list<Item> = []

    let mutable shortCuts : Map<char, Item> =  Map [] // keys (chars) to access items

    let mutable wornItems : WornItems = { Head = Option.None; Hand = Option.None; Torso = Option.None; Legs = Option.None }

    override this.Name 
        with get() = name

    override this.CurrentHP
        with get() = hP

    //Magic : int;  // magic
    //MaxMagic : int;
    
    member this.ShortCuts
        with get() = shortCuts
        and set(value) = shortCuts <- value

    member this.Iron
        with get() = iron
        and set(value) = iron <- value
    
    member this.Gold
        with get() = gold
        and set(value) = gold <- value
    
    member this.Uranium
        with get() = uranium
        and set(value) = uranium <- value

    member this.Water
        with get() = water
        and set(value) = water <- value

    member this.ContaminatedWater
        with get() = contaminatedWater
        and set(value) = contaminatedWater <- value

    override this.SightRadius
        with get() = 10

    member this.Items
        with get() = items
        and set(value) = items <- value

    member this.WornItems
        with get() = wornItems
        and set(value) = wornItems <- value

    override this.IsAlive
        with get() = hP > 0

    override this.HitWithDamage (damage: int) = 
        hP <- hP - damage

    override this.Appearance
        with get() = '@'

    override this.GetMeleeDamage
        with get() = 4

    override this.MaxHP
        with get() = maxHp
 

let createShortCuts currentShortCuts items =
    // use those characters
    let allCharacters = ['a'..'z'] @ ['A'..'Z']        
    // extract used chars
    let usedChars = 
        Map.toSeq currentShortCuts
        |> Seq.map (fun (key, _) -> key)
        |> Seq.toList
    // find out which list is shorter - items or free characters         
    let nToTake = Math.Min(allCharacters.Length - usedChars.Length, (List.length items))
    // find free chars
    let freeChars =         
        let sequence = seq {
            for c in allCharacters do
                let isUsed = List.exists ((=) c) usedChars
                if not isUsed then yield c
        }
        
        Seq.take nToTake sequence |> Seq.toList
    // align item list
    let cutItems = 
        List.toArray items
        |> fun items -> Array.sub items 0 nToTake
        |> Array.toList        

    let newPairs = List.zip freeChars cutItems
    Map.ofList <| Map.toList currentShortCuts @ newPairs

let findShortCut currentShortCuts item = 
    Map.tryFindKey (fun key _item -> _item = item) currentShortCuts