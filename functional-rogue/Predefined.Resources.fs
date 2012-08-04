namespace FunctionalRogue.Predefined

module Resources =

    open FunctionalRogue
    open Parser
    open Board
    open System.Drawing
    open System
    open Replication
    open Predefined.Items
    open Characters


    let startLocationShip board =
        let getStartShipComputer =
            let note1 = { Topic = "Test note 1"; Content = "This is my test note number one." }
            let note2 = { Topic = "Test note 2"; Content = "This is my test note number two. This one is longer. Much, much longer than the one before." }
            let sn = { Topic = "Some test note"; Content = "Nothing interesting here" }
            let lastNote = { Topic = "Last note"; Content = "This is the last note" }
            { ComputerContent = { ComputerName = "Landing Ship Computer"; Notes = [note1; note2; sn; sn; sn; sn; sn; sn; sn; sn; lastNote]; CanOperateDoors = true; CanOperateCameras = true; CanReplicate = false; HasCamera = false; ReplicationRecipes = [allRecipes.[0]; allRecipes.[2]] } }

        let getStartShipReplicator =
            { ComputerContent = { ComputerName = "Universal Replicator"; Notes = []; CanOperateDoors = false; CanOperateCameras = false; CanReplicate = true; HasCamera = false; ReplicationRecipes = [] } }

        let def = def @ [('1', fun _ -> { Place.Floor with Items = [createOreExtractor(); (createEmptyMedicalInjector() |> fillContainerWithLiquid  HealingSolution)]})]
                      @ [('2', fun _ -> { Place.ClosedDoor with ElectronicMachine = Some({ ComputerContent = { ComputerName = "Ship door"; Notes = []; CanOperateDoors = false; CanOperateCameras = false; CanReplicate = false; HasCamera = true; ReplicationRecipes = [] } })})]
                      @ [('3', fun _ -> { Place.Computer with ElectronicMachine = Some(getStartShipComputer)})]
                      @ [('4', fun _ -> { Place.Create Tile.Replicator with ElectronicMachine = Some(getStartShipReplicator)})]
                      @ [('G', fun _ -> { Place.EmptyPlace with Tile = Grass; Features = [OnFire(1)]})]
                      @ [('T', fun _ -> { Place.EmptyPlace with Tile = Tree; Features = [OnFire(1)]})]

        let where = Point(30, 10)
        board
        |> putPredefinedOnBoard def where
                "00##g#,,,,,,TGT
                 0g#1.#,,,,,,GGG
                 gg.3.2,,,,>0TTG
                 0g#4.#,,,,,,GGG
                 00##g#,,,,,,GGT"        

    let randomAncientRuins board =
    
        let ancientRuins1 board =
            let def = def @ [('1', fun _ -> { Place.StairsDown with TransportTarget = Some({ BoardId = Guid.Empty; TargetCoordinates = Point(0,0); TargetLevelType = LevelType.Dungeon })})]

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

        let ancientRuins2 board =
            let def = def @ [('1', fun _ -> { Place.StairsDown with TransportTarget = Some({ BoardId = Guid.Empty; TargetCoordinates = Point(0,0); TargetLevelType = LevelType.Dungeon })})]

            let where = Point(50, 10)
            board
            |> putPredefinedOnBoard def where
                    "________##.##________
                     _______###.###_______
                     ____######.######____
                     __###...........###__
                     __##.............##__
                     ___##.....1.....##___
                     ____##.........##____
                     _____###########_____"   

        let ruinsFunctions = [ancientRuins1;ancientRuins2]

        let index = rnd ruinsFunctions.Length

        board |> ruinsFunctions.[index]