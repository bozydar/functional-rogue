module Player

open Items

type WornItems = {
    Head : int;
    InLeftHand : int;
    InRightHand : int
}

type Player = {
    Name: string;
    HP: int;  // life
    MaxHP: int;
    Magic: int;  // magic
    MaxMagic: int;
    Gold: int // gold
    SightRadius: int // sigh radius
    Items: list<Item>
    WornItems : WornItems
} 