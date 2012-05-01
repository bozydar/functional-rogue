module Replication

open Characters
open Predefined.Items
open Board



let defaultRequiredResources = {
    Iron = 0; Gold = 0; Uranium = 0
}

let allRecipes =
    [{ Name = "Knife"; ResultItem = createKnife(); RequiredResources = { defaultRequiredResources with Iron = 2 } }]
    @[{ Name = "Iron Helmet"; ResultItem = createIronHelmet(); RequiredResources = { defaultRequiredResources with Iron = 4 } } ]
    @[{ Name = "Reconnaissance Drone"; ResultItem = createReconnaissanceDrone(); RequiredResources = { defaultRequiredResources with Iron = 4 } } ]