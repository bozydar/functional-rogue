namespace FunctionalRogue.View

open FunctionalRogue

type ScreenManagerState =
    | MainMenu
    | OptionsMenu
    | BoardScreen
    | EquipmentScreen
    | CharacterScreen
    | UsageScreen of Characters.Item
