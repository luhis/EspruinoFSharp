#load "../espruino.fsx"
open Espruino

#r "../node_modules/fable-core/Fable.Core.dll"
open Fable.Core

let onInit () =
    let f = fun a -> a * 3
    let arr = [|f|]
    let fail = arr.[4] 99
    let okay = arr.[0] 5
    System.Console.WriteLine okay
    System.Console.WriteLine fail
onInit ()