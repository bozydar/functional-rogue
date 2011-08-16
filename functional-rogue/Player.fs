module Player

open Items

type WornItems = {
    Head : option<Item>;
    LeftHand : option<Item>;
    RightHand : option<Item>;
    Torso : option<Item>;
    Legs : option<Item>
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