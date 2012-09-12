namespace FunctionalRogue.View

open FunctionalRogue

type ScreenManagerState =
    | MainMenu
    | OptionsMenu
    | BoardScreen
    | EquipmentScreen
    | UsageScreen of Characters.Item
