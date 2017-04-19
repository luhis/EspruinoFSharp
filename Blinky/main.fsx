#load "../espruino.fsx"
open Espruino

let private getOtherStatus =
    function
    | OutPutStatus.High -> OutPutStatus.Low
    | _ -> OutPutStatus.High

let onInit () :unit =
    let p = NodeMCU.D2
    digitalWrite (p, OutPutStatus.Low)
    let rec flash pin state =
        System.Console.WriteLine "matt"
        digitalWrite (pin, state)
        let newState = getOtherStatus state
        SetTimeout ((fun () -> flash pin newState), 500) |> ignore
    SetTimeout ((fun () -> flash p OutPutStatus.High), 500) |> ignore
onInit ()
//save()