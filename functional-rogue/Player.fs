module Player

open System
open System.Collections.Generic
open Characters
open Quantity

type Player (name : string, hp : int, dexterity : int, strength : int, sightRadius : int, hungerFactorStep : int) = 
    inherit Character (CharacterType.Avatar, hp, dexterity, strength, sightRadius, hungerFactorStep)

    let mutable hP = hp

    let maxHp = hp

    let mutable iron : int = 5

    let mutable gold : int = 0

    let mutable uranium : int = 0

    let mutable water : int = 0

    let mutable contaminatedWater : int = 0

    let mutable shortCuts : Map<char, Item> =  Map [] // keys (chars) to access items    

    override this.Name 
        with get() = name

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

    override this.Appearance
        with get() = '@'

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