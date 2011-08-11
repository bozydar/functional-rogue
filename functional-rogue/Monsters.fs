module Monsters

type Monster (id:int, appearance:char) =
    member this.Id
        with get() = id

    member this.Appearance
        with get() = appearance
