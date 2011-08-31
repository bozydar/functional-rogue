module Quantity

type Quantity = 
    | QuantityValue of int
    | PositiveInfinity
    | NegativeInfinity

    override this.ToString() =
        match this with
        | QuantityValue(value) -> value.ToString()
        | PositiveInfinity | NegativeInfinity-> repr this

    member this.IsInf 
        with get() = 
            match this with
            | PositiveInfinity | NegativeInfinity -> true
            | _ -> false

    member this.Value
        with get() =
            match this with
            | QuantityValue(value) -> value
            | _ -> raise (new System.Exception("this value is inf"))

    static member (+) (a : Quantity, b : Quantity) : Quantity =
        if a = PositiveInfinity && b = NegativeInfinity || a = NegativeInfinity && b = PositiveInfinity then 
            raise (new System.ArgumentOutOfRangeException("a and b cannot be Inf"))
        match a with 
        | QuantityValue(valuea) -> 
            match b with
            | QuantityValue(valueb) -> QuantityValue(valuea + valueb)
            | PositiveInfinity -> PositiveInfinity
            | NegativeInfinity -> NegativeInfinity
        | PositiveInfinity -> PositiveInfinity
        | NegativeInfinity -> NegativeInfinity

    static member (+) (a : Quantity, b : int) : Quantity =
        match a with 
        | QuantityValue(valuea) -> QuantityValue(valuea + b)
        | PositiveInfinity -> PositiveInfinity
        | NegativeInfinity -> NegativeInfinity

    static member (+) (a : int, b : Quantity) : Quantity =
        match b with 
        | QuantityValue(valueb) -> QuantityValue(valueb + a)
        | PositiveInfinity -> PositiveInfinity
        | NegativeInfinity -> NegativeInfinity

    static member (~-) (a : Quantity) : Quantity = 
        match a with
        | QuantityValue(value) -> QuantityValue(-value)
        | PositiveInfinity -> NegativeInfinity
        | NegativeInfinity -> PositiveInfinity

    static member (~+) (a : Quantity) = a        


    static member (-) (a : Quantity, b : Quantity) : Quantity =
        a + (-b)

    static member (-) (a : Quantity, b : int) : Quantity =
        a + (-b)

    static member (-) (a : int, b : Quantity) : Quantity =
        a + (-b)

//System.Double.NegativeInfinity
//type Int32 = 
    