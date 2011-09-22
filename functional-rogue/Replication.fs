module Replication

open Characters
open Predefined.Items
open Board

type ReplicationRecipe = {
    Name : string
    ResultItem : PredefinedItems
    RequiredResources : RequiredResources
}
and RequiredResources = {
    Iron : int
    Gold : int
    Uranium : int
}

let defaultRequiredResources = {
    Iron = 0; Gold = 0; Uranium = 0
}

let allRecipes =
    [{ Name = "Knife"; ResultItem = Knife; RequiredResources = { defaultRequiredResources with Iron = 2 } }]