#load "../espruino.fsx"
open Espruino

#r "../node_modules/fable-core/Fable.Core.dll"
open Fable.Core

//[<Erase>]
let onInit () :unit =
    let f = fun a -> a * 3
    let arr = [|f|]
    let res = arr.[4] 5
    System.Console.WriteLine res
onInit ()