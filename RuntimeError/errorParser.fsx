let lines = System.IO.File.ReadAllLines @"./error.txt"

let errorLines = lines |> Seq.filter (fun a -> a.Contains("line") && a.Contains("col"))

type Location = {line:int; col:int}

open System.Text.RegularExpressions
let pattern = Regex @".*line (?<line>\d+) col (?<col>\d+).*"

let getLocation (line:string) : Option<Location> =
    let matches = pattern.Match line
    match matches.Success with
    | true -> Some {line= matches.Groups.[1].Value |> int; col= matches.Groups.[2].Value |> int}
    | false -> None

errorLines |> Seq.choose getLocation |> Seq.iter (fun a -> printfn "line %d col %d" a.line a.col )

#r @"../FSharp.Data.dll"
open FSharp.Data
type Simple = JsonProvider< @"./out/bundle.js.map" >

type Segment = {outPutColumn:int; fileIndex:int; inputLine:int; inputColumn:int; nameIndex:option<int>; outPutLine:int}

let map = Simple.Load @"./out/bundle.js.map"
map.Version |> System.Console.WriteLine
let groupings = map.Mappings.Split(';')

// http://www.murzwin.com/base64vlq.html

let Expect0088 = "AAQQ"

let base64MimeToInt =
    function
    | c when c >= 'A' && c <= 'Z' -> (uint8 c) - (uint8 'A')
    | c when c >= 'a' && c <= 'z' -> (uint8 c) - (uint8 'a') + 26uy
    | c when c >= '0' && c <= '9' -> (uint8 c) - (uint8 '0') + 52uy
    | '+' -> 62uy
    | '/' -> 63uy
    | _ -> 0uy

type Vlq = {isContinuation:bool; v:uint8; isNegative:bool}
type VlqContinuation = {isContinuation:bool; v:uint8}

let toInt (v:Vlq) =
    let multiplier =
        match v.isNegative with
        | true -> -1
        | false -> 1
    (int v.v) * multiplier

let toInts (v:List<Vlq>) =
    let rec proc (carry:int) (v:List<Vlq>) : List<int> = 
        let prevWasCont = carry <> 0
        match v with
        | x::xs -> 
            let itemVal =
                match prevWasCont with
                | true -> 
                    let negPoint =
                        match x.isNegative with 
                        | true -> 1uy
                        | false -> 0uy
                    int <| ((x.v) <<< 1) + negPoint
                | _ -> (toInt x)
            match x.isContinuation with
            | false ->  (toInt x) + carry::(proc 0 xs)
            | true -> (proc (toInt x + carry) xs)
        | [] -> []
    v |> List.rev |> proc 0

let toVlq (i:uint8) =
    let isNegative = (i &&& 0x01uy)= 0x01uy
    let v = (i &&& 0xFEuy) >>> 1
    let isContinuation = (i >>> 5) = 0x01uy
    {isContinuation=isContinuation; v=v; isNegative=isNegative}

let decode (s:string) =
    let chars = s.ToCharArray() |> Seq.toList
    chars |> List.map (base64MimeToInt >> toVlq >> toInt)

let toSegment line (previousItem:option<Segment>) (s:List<int>) : option<Segment> =
    let inputLineOffSet =
        match previousItem with
        | Some a -> a.outPutLine
        | None -> 0
    match s with
    | outPutCol::fileIndex::inputLine::[inputCol] -> 
        Some {outPutColumn=outPutCol; outPutLine=(line); fileIndex=fileIndex; inputColumn=inputCol; inputLine=inputLineOffSet + inputLine; nameIndex=None}
    | _ -> None
    

//let segments = groupings |> Seq.mapi (fun i a -> (decode >> (toSegment i) ) a) |> Seq.choose id
let segments : List<Segment> = groupings |> 
    Seq.mapi (fun i a -> i, a) |> 
    Seq.fold (
        fun (acc:List<Segment>) (index, item) -> 
            let newVal = (item |> decode |> toSegment index (Seq.tryHead acc) )
            match newVal with
            | Some v -> v::acc
            | _ -> acc
    ) [] |> List.rev

//printfn "Result: %i" <| Seq.length segments
Seq.iter (fun a -> printfn "[%d,%d](#%d)=>[%d,%d]" a.inputLine a.inputColumn a.fileIndex a.outPutLine a.outPutColumn) segments