module Replication

open Characters
open Predefined.Items
open Board

type ReplicationRecipe = {
    Name : string
    ResultItem : Item
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
    [{ Name = "Knife"; ResultItem = knife; RequiredResources = { defaultRequiredResources with Iron = 2 } }]