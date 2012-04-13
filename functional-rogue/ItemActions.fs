module ItemActions

open Characters
open State

let performUseItemAction (item : Item) (state : State)=
    state