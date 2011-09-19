module Replication

open Items

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
    [{ Name = "Combat Knife"; ResultItem = CombatKnife; RequiredResources = { defaultRequiredResources with Iron = 2 } }]