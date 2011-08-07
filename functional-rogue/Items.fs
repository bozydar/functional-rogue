module Items

type ItemProperties = {
    Name: string
    Weight: decimal;
    Description: string
}

type Item = 
    | Plain of int * ItemProperties // id and properties
    | Gold of int

let all = 
    List.mapi (fun i item -> Plain(i + 1, item)) 
    <| [
        {Name = "Wooden staff"; Weight = 1.5M; Description = "Simple wooden staff"}
        {Name = "Screwdriver"; Weight = 0.1M; Description = "Steel screwdriver"}
    ]