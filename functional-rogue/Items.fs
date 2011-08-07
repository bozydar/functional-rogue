module Items

type Item = 
    | Sword
    | Vand
    | Gold of int

type ItemProperties = {
    Weight: int;
    Name: string
    Description: string
}
