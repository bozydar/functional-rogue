module Predefined.Resources

open Parser
open Board
open System.Drawing
open System


let startLocationShip board =
    let getStartShipComputer =
        let note1 = { Topic = "Test note 1"; Content = "This is my test note number one." }
        let note2 = { Topic = "Test note 2"; Content = "This is my test note number two. This one is longer." }
        let sn = { Topic = "Some test note"; Content = "Nothing interesting here" }
        let lastNote = { Topic = "Last note"; Content = "This is the last note" }
        { ComputerContent = { ComputerName = "TestComp"; Notes = [note1; note2; sn; sn; sn; sn; sn; sn; sn; sn; lastNote]; CanOperateDoors = true; HasCamera = false } }

    let def = def @ [('1', { Place.Floor with Items = [Items.createPredefinedItem Items.OreExtractor]})]
                  @ [('2', { Place.ClosedDoor with ElectronicMachine = Some({ ComputerContent = { ComputerName = "Ship door"; Notes = []; CanOperateDoors = false; HasCamera = true } })})]
                  @ [('3', { Place.Computer with ElectronicMachine = Some(getStartShipComputer)})]

    let where = Point(30, 10)
    board
    |> putPredefinedOnBoard def where
            "00##g#,,,,,,
             0g#1.#,,,,,,
             gg.3.2,,,,>0
             0g#..#,,,,,,
             00##g#,,,,,,"        
