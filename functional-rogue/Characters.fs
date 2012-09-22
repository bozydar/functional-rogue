namespace FunctionalRogue

module Characters =

    open System
    open Quantity
    open System.Drawing
    open Config

    type damage = (int * int * int)

    [<Measure>] type ml

    type AttackResult = {
        Damage : damage;
        AttackBonus : int;
        DefenceBonus : int
    }

    let scratchWound = 1
    let lightWound = 3
    let heavyWound = 9
    let criticalWound = 27


    type CharacterType = 
        | Avatar
        | Monster
        | NPC

    [<AbstractClass>]
    type Character (characterType: CharacterType, startingHP: int, startingDexterity : int, startingStrength : int, startingSightRadius : int, startingHungerFactorStep : int) =
        let id = System.Guid.NewGuid ()
        let mutable hp = startingHP
        let mutable maxHP = startingHP
        let mutable sightRadius = startingSightRadius
        let mutable dexterity = startingDexterity
        let mutable strength = startingStrength
        let mutable involvedInFight = false
        let mutable hungerFactor = 0
        let mutable hungerFactorStep = startingHungerFactorStep
        let mutable sightRadiusMultiplier = 1
        let mutable canSeeThroughWalls = false

        let mutable items : list<Item> = []

        let mutable wornItems : WornItems = { Head = Option.None; Hand = Option.None; Torso = Option.None; Legs = Option.None }

        member this.Items
            with get() = items
            and set(value) = items <- value

        member this.WornItems
            with get() = wornItems
            and set(value) = wornItems <- value


        member this.Type
            with get() = characterType

        member this.Dexterity 
            with get() = dexterity
            and set(value) = dexterity <- value

        member this.Strength
            with get() = strength
            and set(value) = strength <- value
    
        member this.CurrentHP 
            with get() = hp
            and set(value) = hp <- value

        member this.MaxHP 
            with get() = maxHP
            and set(value) = maxHP <- value

        member this.SightRadiusMultiplier
            with get() = sightRadiusMultiplier
            and set(value) = sightRadiusMultiplier <- value

        member this.CanSeeThroughWalls
            with get() = canSeeThroughWalls
            and set(value) = canSeeThroughWalls <- value

        member this.InvolvedInFight
            with get() = involvedInFight
            and set(value) = involvedInFight <- value

        member this.ResetVolatileStates () =
            this.InvolvedInFight <- false

        member this.TickBiologicalClock () =
            hungerFactor <- hungerFactor + 1
            match this.HungerLevel with
            | 1 when rnd 100 < 3 -> this.HitWithDamage 1
            | 2 when rnd 100 < 10   -> this.HitWithDamage 1
            | 3 when rnd 100 < 20   -> this.HitWithDamage 1
            | _ -> ()

        member this.HungerLevel =         
            if isInBoundary this.HungerFactor hungerFactorStep (hungerFactorStep * 2) then 1
            elif isInBoundary this.HungerFactor (hungerFactorStep * 2) (hungerFactorStep * 3) then 2
            elif this.HungerFactor > (hungerFactorStep * 3) then 3
            else 0

        member this.HungerFactor
            with get() = hungerFactor
            and set(value) = hungerFactor <- value

        member this.Eat (item : Item) =
            hungerFactor <- Math.Min(0, hungerFactor - if item.IsEatable then -100 else 0)

        member this.Wear (item : Item) =
            match item.Wearing with
            | OnHead -> this.WornItems <- {this.WornItems with Head = Some item }
            | OnLegs -> this.WornItems <- {this.WornItems with Legs = Some item }
            | OnTorso -> this.WornItems <- {this.WornItems with Torso = Some item }
            | InHand -> this.WornItems <- {this.WornItems with Hand = Some item }
            | _ -> ()

        member this.TakeOff (item : Item) = 
            let item = Some item
            if item = this.WornItems.Hand then this.WornItems <- {this.WornItems with Hand = None }
            if item = this.WornItems.Head then this.WornItems <- {this.WornItems with Head = None }
            if item = this.WornItems.Torso then this.WornItems <- {this.WornItems with Torso = None }
            if item = this.WornItems.Legs then this.WornItems <- {this.WornItems with Legs = None }

        abstract member MeleeAttack : AttackResult with get
        default this.MeleeAttack
            with get() =
                if this.WornItems.Hand.IsSome then
                    match this.WornItems.Hand.Value.Attack with
                    | Attack(attackFunc) -> attackFunc this this 1
                    | Cannot -> this.DefaultMeleeAttack
                else
                    this.DefaultMeleeAttack
    
        abstract member DefaultMeleeAttack : AttackResult with get
        default this.DefaultMeleeAttack 
            with get() = 
                let result = { Damage = (0, 0, 0); AttackBonus = 0; DefenceBonus = 0 }
                if this.Strength < 10 then { result with Damage = scratchWound, scratchWound, scratchWound }
                elif this.Strength < 12 then { result with Damage = scratchWound, scratchWound, lightWound }
                elif this.Strength < 14 then { result with Damage = scratchWound, lightWound, lightWound }
                elif this.Strength < 16 then { result with Damage = scratchWound, lightWound, heavyWound }
                elif this.Strength < 18 then { result with Damage = lightWound, lightWound, heavyWound }
                else { result with Damage = lightWound, heavyWound, heavyWound }

        member this.SightRadius  
            with get() = sightRadius * sightRadiusMultiplier
            and set(value) = sightRadius <- value

        abstract Appearance : char with get

        member this.IsAlive
            with get() : bool = this.CurrentHP > 0    

        abstract Name : string with get

        member this.HitWithDamage (value : int)  =
            this.CurrentHP <- this.CurrentHP - value

    and WornItems = {
        Head : option<Item>;
        Hand : option<Item>;    
        Torso : option<Item>;
        Legs : option<Item>
    } with
        member this.IsWorn(item) =
           [this.Hand; this.Head; this.Torso; this.Legs] |> List.exists ((=)(Some item))
            
    and 
        Item (name: string, image : string, wearing: Wearing, _type: Type,
                initialContainer : Container option ) =
        let id = Guid.NewGuid()
        let mutable container = initialContainer

        member this.Id
            with get() : Guid = id 
   
        member this.Name
            with get() : string = name

        member this.Image
            with get() : string = image

        member this.Wearing 
            with get() : Wearing = wearing

        member this.Type
            with get() : Type = _type

        member this.Attack 
            with get() : Attack =
                match _type with
                | Weapon(weapon) -> weapon.Attack
                | _ -> Attack.Cannot

        member this.IsWearable
            with get() : bool = this.Wearing <> Wearing.NotWearable

        member this.IsEatable
            with get() : bool = 
                match this.Type with
                | Type.Corpse -> true
                | _ -> false

        //container section

        member this.IsLiquidContainer
            with get() : bool = container.IsSome && container.Value.LiquidCapacity > 0.0<ml>

        member this.Container
            with get() = container

        member this.AddLiquid (liquid : Liquid) =
            if this.IsLiquidContainer && (this.Container.Value.LiquidInside.IsNone || this.Container.Value.LiquidInside.Value.Type = liquid.Type) then
                let spaceAvailable = container.Value.LiquidCapacity - (if container.Value.LiquidInside.IsSome then container.Value.LiquidInside.Value.Amount else 0.0<ml>)
                let newLiquidInsideAmount = (if container.Value.LiquidInside.IsSome then container.Value.LiquidInside.Value.Amount else 0.0<ml>) + (if spaceAvailable < liquid.Amount then spaceAvailable else liquid.Amount)
                let newLiquidOutsideAmount = if spaceAvailable > liquid.Amount then 0.0<ml> else liquid.Amount - spaceAvailable
                let newLiquidInsideType = liquid.Type
                container <- Some( { container.Value with LiquidInside = Some( { Type = newLiquidInsideType; Amount = newLiquidInsideAmount} ) } )
                if newLiquidOutsideAmount > 0.0<ml> then Some({ Type = liquid.Type; Amount = newLiquidOutsideAmount}) else None
            else
                Some(liquid)

        member this.TakeLiquid (amount : float<ml>) =
            if this.IsLiquidContainer && container.Value.LiquidInside.IsSome then
                if amount > container.Value.LiquidInside.Value.Amount then
                    let result = container.Value.LiquidInside
                    container <- Some( {container.Value with LiquidInside = None} )
                    result
                else
                    let resultLiquid = { Type = container.Value.LiquidInside.Value.Type; Amount = amount }
                    let liquidLeft = { Type = container.Value.LiquidInside.Value.Type; Amount = container.Value.LiquidInside.Value.Amount - amount }
                    container <- Some( {container.Value with LiquidInside = if liquidLeft.Amount > 0.0<ml> then Some(liquidLeft) else None } )
                    Some(resultLiquid)
            else
                None
        //end container section

        override this.Equals(other) =
            match other with
            | :? Item as other -> other.Id = id
            | _ -> false
    
        override this.GetHashCode() = 
            hash this.Id

        interface IComparable with
            member this.CompareTo(other) =
                match other with
                | :? Item as other -> compare this other
                | _ -> invalidArg "other" "cannot compare values of different types"        

    and Attack = 
        | Attack of (Character -> Character -> int -> AttackResult)
        | Cannot

    and Wearing =
        | NotWearable
        | OnHead 
        | InHand 
        | OnTorso
        | OnLegs 
     
    and Type = 
        | Weapon of Weapon
        | Armor of Armor
        | Rock
        | Injector // of Liquid
        | Corpse
        | OreExtractor of OreExtractorProperties
        | Drone
        | SimpleContainer

    and OreExtractorProperties = { HarvestRate: int }

    and Weapon = { 
        Attack : Attack
        Type : WeaponType  }

    and WeaponType = 
        | Stick
        | Knife
        | Sword
    
    and Armor = { 
        Defence : Attack (* should be change to it's own type *)
    }

    and Liquid = {
        Type : LiquidType
        Amount : float<ml>
    }

    and LiquidType =
        | Water
        | HealingSolution

    and Container = {
        LiquidCapacity : float<ml>
        LiquidInside : Liquid option
    } 
    and Tile =
        | Wall 
        | Floor
        | Empty
        | OpenDoor
        | ClosedDoor
        | Grass
        | Tree
        | SmallPlants
        | Bush
        | Glass
        | Sand
        | Water
        | StairsDown
        | StairsUp
        | Computer
        | Replicator
        | MainMapForest
        | MainMapGrassland
        | MainMapWater
        | MainMapMountains
        | MainMapCoast

    and LevelType = 
        | Test
        | Dungeon
        | Cave
        | Forest
        | Empty
        | Grassland
        | Coast
        | MainMap

    and TransportTarget = {
        BoardId : Guid;
        TargetCoordinates : Point
        TargetLevelType : LevelType
    }   

    and Ore = 
        | NoneOre
        | Iron of Quantity
        | Gold of Quantity
        | Uranium of Quantity
        | CleanWater of Quantity
        | ContaminatedWater of Quantity 
        member this.Quantity 
            with get() = 
                match this with
                | Iron(value) -> value
                | Gold(value) -> value
                | Uranium(value) -> value
                | CleanWater(value) -> value
                | ContaminatedWater(value) -> value
                | _ -> QuantityValue(0)

    and ReplicationRecipe = {
        Name : string
        ResultItem : Item
        RequiredResources : RequiredResources
    }
    and RequiredResources = {
        Iron : int
        Gold : int
        Uranium : int
    }
    and ElectronicMachine = {
        ComputerContent : ComputerContent
    }
    and ComputerContent = {
        ComputerName : string;
        Notes : ComputerNote list;
        CanOperateDoors : bool;
        CanOperateCameras : bool;
        CanReplicate : bool;
        HasCamera : bool;
        ReplicationRecipes : ReplicationRecipe list;
    }
    and ComputerNote = {
        Topic : string;
        Content : string;
    }

    and PlaceFeature =
        | OnFire of int

    and Place = {
        Tile : Tile; 
        Items : Item list;
        Ore : Ore
        Character : Character option;    
        IsSeen : bool;
        WasSeen : bool;
        TransportTarget : TransportTarget option;
        ElectronicMachine : ElectronicMachine option;
        Features : PlaceFeature list
    } with
        static member EmptyPlace = 
                {Tile = Tile.Empty; Items = []; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; Ore = NoneOre; TransportTarget = None; ElectronicMachine = None; Features = [] }
        static member Wall = 
                {Tile = Tile.Wall; Items = []; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; Ore = NoneOre; TransportTarget = None; ElectronicMachine = None; Features = [] }
        static member Floor = 
                {Tile = Tile.Floor; Items = []; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; Ore = NoneOre; TransportTarget = None; ElectronicMachine = None; Features = [] }
        static member StairsDown = 
                {Tile = Tile.StairsDown; Items = []; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; Ore = NoneOre; TransportTarget = None; ElectronicMachine = None; Features = [] }
        static member ClosedDoor =
                {Tile = Tile.ClosedDoor; Items = []; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; Ore = NoneOre; TransportTarget = None; ElectronicMachine = None; Features = [] }
        static member Computer =
                {Tile = Tile.Computer; Items = []; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; Ore = NoneOre; TransportTarget = None; ElectronicMachine = None; Features = [] }
        static member Create tile =
            {Tile = tile; Items = []; Character = Option.None; IsSeen = false; WasSeen = Settings.EntireLevelSeen; Ore = NoneOre; TransportTarget = None; ElectronicMachine = None; Features = [] }
        static member GetDescription (place: Place) additionalDescription =
            let tileDescription = 
                match place.Tile with
                | Tile.Floor -> "Floor."
                | Tile.Wall -> "Wall."
                | Tile.Glass -> "Glass."
                | Tile.Grass -> "Grass."
                | Tile.Bush -> "Some bushes."
                | Tile.SmallPlants -> "Some plants."
                | Tile.Tree -> "Tree."
                | Tile.Sand -> "Sand."
                | Tile.Water -> "Water."
                | Tile.ClosedDoor -> "Closed door."
                | Tile.OpenDoor -> "Open door."
                | Tile.StairsDown -> "Stairs leading down."
                | Tile.StairsUp -> "Stairs leading up."
                | Tile.Computer -> "Computer."
                | Tile.Replicator -> "Replicator."
                | Tile.MainMapForest -> "Forest." + additionalDescription
                | Tile.MainMapCoast -> "Coast." + additionalDescription
                | Tile.MainMapGrassland -> "Grassland." + additionalDescription
                | Tile.MainMapMountains -> "Mountains." + additionalDescription
                | Tile.MainMapWater -> "Water." + additionalDescription
                | _ -> ""
            let characterDescription =
                if place.Character.IsSome then
                    " " + place.Character.Value.Name + " is standing here."
                else
                    ""
            let itemsDescription =
                if place.Items.Length > 1 then
                    " Some items are lying here."
                elif place.Items.Length > 0 then
                    " " + place.Items.Head.Name + " is lying here."
                else
                    ""
            tileDescription + characterDescription + itemsDescription


    let itemShortDescription (item: Item)=
        let rest = 
            let liqiudContainerDescription =
                if item.IsLiquidContainer then 
                    if item.Container.Value.LiquidInside.IsSome then
                        " (" + item.Container.Value.LiquidInside.Value.Amount.ToString() + "ml of " + (getUnionCaseName item.Container.Value.LiquidInside.Value.Type) + ")"
                    else
                        "(empty)"
                else
                    ""
            item.Name + liqiudContainerDescription
        rest