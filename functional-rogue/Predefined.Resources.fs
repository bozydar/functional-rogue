module Predefined.Resources

open Parser
open Board
open System.Drawing
open System


let startLocationShip board =
    let getStartShipComputer =
        let note1 = { Topic = "Test note 1"; Content = "This is my test note number one." }
        let note2 = { Topic = "Test note 2"; Content = "This is my test note number two. This one is longer. Much, much longer than the one before." }
        let sn = { Topic = "Some test note"; Content = "Nothing interesting here" }
        let lastNote = { Topic = "Last note"; Content = "This is the last note" }
        { ComputerContent = { ComputerName = "Landing Ship Computer"; Notes = [note1; note2; sn; sn; sn; sn; sn; sn; sn; sn; lastNote]; CanOperateDoors = true; CanOperateCameras = true; CanReplicate = false; HasCamera = false } }

    let getStartShipReplicator =
        { ComputerContent = { ComputerName = "Universal Replicator"; Notes = []; CanOperateDoors = false; CanOperateCameras = false; CanReplicate = true; HasCamera = false } }

    let def = def @ [('1', { Place.Floor with Items = [Items.createPredefinedItem Items.OreExtractor]})]
                  @ [('2', { Place.ClosedDoor with ElectronicMachine = Some({ ComputerContent = { ComputerName = "Ship door"; Notes = []; CanOperateDoors = false; CanOperateCameras = false; CanReplicate = false; HasCamera = true } })})]
                  @ [('3', { Place.Computer with ElectronicMachine = Some(getStartShipComputer)})]
                  @ [('4', { Place.Create Tile.Replicator with ElectronicMachine = Some(getStartShipReplicator)})]

    let where = Point(30, 10)
    board
    |> putPredefinedOnBoard def where
            "00##g#,,,,,,
             0g#1.#,,,,,,
             gg.3.2,,,,>0
             0g#4.#,,,,,,
             00##g#,,,,,,"        

let ancientRuins board =
    let def = def @ [('1', { Place.StairsDown with TransportTarget = Some({ BoardId = Guid.Empty; TargetCoordinates = Point(0,0); TargetLevelType = LevelType.Dungeon })})]

    let where = Point(50, 10)
    board
    |> putPredefinedOnBoard def where
            "##########.##########
             ###...............###
             #...................#
             #...................#
             ..........1..........
             #...................#
             #...................#
             ###...............###
             ##########.##########"      
