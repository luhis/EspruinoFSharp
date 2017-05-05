let lines = System.IO.File.ReadAllLines @"./error.txt" |> Seq.toList

let errorLines = lines |> List.filter (fun a -> a.Contains("line") && a.Contains("col"))

type Location = {line:int; col:int}

open System.Text.RegularExpressions
let pattern = Regex @".*line (?<line>\d+) col (?<col>\d+).*"

let getLocation line =
    let matches = pattern.Match line
    match matches.Success with
    | true -> Some {line= matches.Groups.[1].Value |> int; col= matches.Groups.[2].Value |> int}
    | false -> None

let errorItems = errorLines |> List.choose getLocation 
errorItems |> Seq.iter (fun a -> printfn "line %d col %d" a.line a.col )

#r @"../FSharp.Data.dll"
open FSharp.Data
type Simple = JsonProvider< @"./out/bundle.js.map" >

type Segment = {inputLine:int; inputColumn:int; fileIndex:int; outPutLine:int; outPutColumn:int; nameIndex:option<int>}

let map = Simple.Load @"./out/bundle.js.map"
let groupings = map.Mappings.Split(';')

// http://www.murzwin.com/base64vlq.html

let base64MimeToInt =
    function
    | c when c >= 'A' && c <= 'Z' -> (uint8 c) - (uint8 'A')
    | c when c >= 'a' && c <= 'z' -> (uint8 c) - (uint8 'a') + 26uy
    | c when c >= '0' && c <= '9' -> (uint8 c) - (uint8 '0') + 52uy
    | '+' -> 62uy
    | '/' -> 63uy
    | _ -> 0uy

let toInts v =
    let rec proc carry v = 
        match v with
        | x::xs -> 
            let isContinuation = ((x &&& 0x20uy) >>> 5) = 0x01uy
            let itemVal =
                match carry with
                | Some a -> 
                    let multiplier =
                        match a >= 0 with
                        | true -> 1
                        | _ -> -1
                    // the shift here by 4 is not good. sometimes it will be 5. this needs to be invered or something
                    ((int <| ((x &&& 0x1Fuy)<<< 4)) * multiplier) + (a )
                | None -> 
                    let v = (x &&& 0x1Euy) >>> 1
                    let isNegative = (x &&& 0x01uy)= 0x01uy
                    let multiplier =
                        match isNegative with
                        | true -> -1
                        | false -> 1
                    (int v) * multiplier
            match isContinuation with
            | false ->  itemVal::(proc None xs)
            | true -> (proc (Some itemVal) xs)
        | [] -> []
    v |> proc None

let decode (s:string) =
    s.ToCharArray() |> Seq.toList |> List.map (base64MimeToInt) |> toInts

let toSegment line previousItem s =
    let inputLineOffSet =
        match previousItem with
        | Some a -> a.inputLine
        | None -> 0
    //printfn "%A" s
    match s with
    | outPutCol::fileIndex::inputLine::[inputCol] -> 
        Some {outPutColumn=outPutCol; outPutLine=(line); fileIndex=fileIndex; inputColumn=inputCol; inputLine=inputLineOffSet + inputLine; nameIndex=None}
    | _ -> None
    
let segments = 
    groupings |> 
    Seq.mapi (fun i a -> i, a) |> 
    Seq.fold (
        fun acc (index, item) -> 
            let newVal = (item |> decode |> toSegment index (Seq.tryHead acc) )
            match newVal with
            | Some v -> v::acc
            | _ -> acc
    ) [] |> List.rev

let print segment =
    printfn "[%d,%d](#%d)=>[%d,%d]" segment.inputLine segment.inputColumn segment.fileIndex segment.outPutLine segment.outPutColumn

Seq.iter print segments

let firstError = errorItems |> List.head
let closestMapItem = segments |> List.filter (fun a -> a.outPutLine <= firstError.line && a.outPutColumn <= firstError.col) |> List.sortByDescending (fun a -> a.inputColumn) |> List.head

///print closestMapItem

let fileName = map.Sources.[closestMapItem.fileIndex].Replace("..", ".")
let file = System.IO.File.ReadAllLines fileName

printfn "error line:\n%s" file.[closestMapItem.inputLine + 1 + 1]
printfn "%s^" <| ((Seq.init closestMapItem.inputColumn (fun _ -> ' ')) |> Array.ofSeq |> System.String)