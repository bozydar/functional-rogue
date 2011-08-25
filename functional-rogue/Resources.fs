module Resources

open System.IO

type ResourceManager private()=
    static let instance = new ResourceManager()
    
    let rec getObjectLinesAndTheRest (lines: string list) =
        match lines with
        | head :: tail ->
            if not(head.StartsWith("=")) then
                let restResult = getObjectLinesAndTheRest tail
                ( (lines.Head :: (fst restResult)),(snd restResult))
            else
                ([],head :: tail)
        | _ -> ([],[])

    let rec getObjects (lines: string list) =
        match lines with
        | head :: tail ->
            if not(head.StartsWith("=")) then
                failwith "Invalid map_resources format"
            let name = lines.Head.Substring(1)
            let objectLinesAndRest = getObjectLinesAndTheRest lines.Tail
            (name, (fst objectLinesAndRest)) :: getObjects (snd objectLinesAndRest)
        | _ -> []
    
    let stringListToArray (strings: string list) =
        let length1 = strings.Head.Length
        let length2 = strings.Length
        Array2D.init length1 length2 (fun x y -> strings.[y].Chars x)
       
    let rec createDictionaryObjects (rawObjects: (string*string list)list) (dict: System.Collections.Generic.Dictionary<string,char[,]>) =
        match rawObjects with
        | head :: tail ->
            dict.Add((fst head), (stringListToArray (snd head)))
            createDictionaryObjects tail dict
        | _ -> dict

    let allLines = File.ReadAllLines("map_resources.txt")
    let linesList = (List.ofArray allLines) |> List.filter (fun line -> not(System.String.IsNullOrEmpty(line)) && not(line.StartsWith("//"))) 
    let rawObjects = getObjects linesList
    let simplifiedObjects = createDictionaryObjects rawObjects (new System.Collections.Generic.Dictionary<string,char[,]>())
    
    static member Instance = instance

    member this.SimplifiedMapObjects
        with get() = simplifiedObjects