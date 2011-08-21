module Player

open System
open System.Collections.Generic
open Items

type WornItems = {
    Head : option<Item>;
    Hand : option<Item>;    
    Torso : option<Item>;
    Legs : option<Item>
} 

type Player = {
    Name : string;
    HP : int;  // life
    MaxHP : int;
    Magic : int;  // magic
    MaxMagic : int;
    Iron : int
    Gold : int // gold
    Uranium : int
    SightRadius : int // sigh radius
    Items : list<Item>
    ShortCuts : Map<char, Item> // keys (chars) to access items
    WornItems : WornItems
} 

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