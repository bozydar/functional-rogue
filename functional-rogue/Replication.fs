module Replication

open Characters
open Predefined.Items
open Board



let defaultRequiredResources = {
    Iron = 0; Gold = 0; Uranium = 0
}

let allRecipes =
    [{ Name = "Knife"; ResultItem = knife; RequiredResources = { defaultRequiredResources with Iron = 2 } }]
    @[{ Name = "Iron Helmet"; ResultItem = ironHelmet; RequiredResources = { defaultRequiredResources with Iron = 4 } } ]