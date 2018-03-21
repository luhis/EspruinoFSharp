#load "../espruino.fsx"
open Espruino

let getOtherStatus =
    function
    | OutPutStatus.High -> OutPutStatus.Low
    | _ -> OutPutStatus.High

let onInit () =
    let p = NodeMCU.D2
    digitalWrite (p, OutPutStatus.Low)
    let rec flash state =
        digitalWrite (p, state)
        let newState = getOtherStatus state
        SetTimeout ((fun () -> flash newState), 500) |> ignore
    SetTimeout ((fun () -> flash OutPutStatus.High), 500) |> ignore
onInit ()
