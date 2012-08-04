namespace FunctionalRogue.Predefined

[<RequireQualifiedAccessAttribute>]
module Help = 

    open FunctionalRogue
    open System.Text.RegularExpressions
    open DialogBuilder

    let _commands = [
            "Up, 8", "Go north";
            "Down, 2", "Go south";
            "Left, 4", "Go west";
            "Right, 6", "Go east";
            "7", "Go north-west";
            "1", "Go south-west";
            "3", "Go south-east";
            "9", "Go north-east";
            "Comma", "Take";
            "i", "Show items";
            "Escape", "Save & Quit";
            "o", "Open/Close doors";
            "e", "Show equipment";
            "E", "Eat";
            "m", "Show list of messages";
            "h", "Harvest ore";
            "W", "Wear shield, armory or weapon";
            "T", "Take off shield, armory or weapon";
            ">", "Go down or start explore area";
            "<", "Go up";
            "d", "Drop item";
            "l", "Look";
            "U", "Use object which is on the board";
            "u", "Use item from your inventory";
            "O", "???";
            "Ctrl-P", "Pour liquid";
            "?", "This help";
        ]


    let commands = 
        new DialogBuilder()
        => FG.Green =>  "Up, 8"    => FG.Gray =>  " - Go north"                           => "\n"
        => FG.Green =>  "Down, 2"  => FG.Gray =>  " - Go south"                          => "\n"
        => FG.Green =>  "Left, 4"  => FG.Gray =>  " - Go west"                           => "\n"
        => FG.Green =>  "Right, 6" => FG.Gray =>  " - Go east"                           => "\n"
        => FG.Green =>  "7"        => FG.Gray =>  " - Go north-west"                     => "\n"
        => FG.Green =>  "1"        => FG.Gray =>  " - Go south-west"                     => "\n"
        => FG.Green =>  "3"        => FG.Gray =>  " - Go south-east"                     => "\n"
        => FG.Green =>  "9"        => FG.Gray =>  " - Go north-east"                     => "\n"
        => FG.Green =>  "Comma"    => FG.Gray =>  " - Take"                              => "\n"
        => FG.Green =>  "i"        => FG.Gray =>  " - Show items"                        => "\n"
        => FG.Green =>  "Escape"   => FG.Gray =>  " - Save & Quit"                       => "\n"
        => FG.Green =>  "o"        => FG.Gray =>  " - Open/Close doors"                  => "\n"
        => FG.Green =>  "e"        => FG.Gray =>  " - Show equipment"                    => "\n"
        => FG.Green =>  "E"        => FG.Gray =>  " - Eat"                               => "\n"
        => FG.Green =>  "m"        => FG.Gray =>  " - Show list of messages"             => "\n"
        => FG.Green =>  "h"        => FG.Gray =>  " - Harvest ore"                       => "\n"
        => FG.Green =>  "W"        => FG.Gray =>  " - Wear shield, armory or weapon"     => "\n"
        => FG.Green =>  "T"        => FG.Gray =>  " - Take off shield, armory or weapon" => "\n"
        => FG.Green =>  ">"        => FG.Gray =>  " - Go down or start explore area"     => "\n"
        => FG.Green =>  "<"        => FG.Gray =>  " - Go up"                             => "\n"
        => FG.Green =>  "d"        => FG.Gray =>  " - Drop item"                         => "\n"
        => FG.Green =>  "l"        => FG.Gray =>  " - Look"                              => "\n"
        => FG.Green =>  "U"        => FG.Gray =>  " - Use object which is on the board"  => "\n"
        => FG.Green =>  "u"        => FG.Gray =>  " - Use item from your inventory"      => "\n"
        => FG.Green =>  "O"        => FG.Gray =>  " - ???"                               => "\n"
        => FG.Green =>  "Ctrl-P"   => FG.Gray =>  " - Pour liquid"                       => "\n"
        => FG.Green =>  "?"        => FG.Gray =>  " - This help"                         => "\n"
