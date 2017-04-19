#r "./node_modules/fable-core/Fable.Core.dll"
open Fable.Core
open System
open System.Collections.Generic
#load "../espruino.fsx"
open Espruino
#load "../wifiSettings.fsx"

System.Console.WriteLine ("Initialising. Serial " + Espruino.getSerial ())

let mutable private state = Espruino.OutPutStatus.High

let private getOtherStatus s =
    match s with
    | Espruino.OutPutStatus.High -> Espruino.OutPutStatus.Low
    | _ -> Espruino.OutPutStatus.High

[<Erase>]
type IParameters =
    [<Emit("$0.led")>]
    member x.Led : string = jsNative

let onInit () : unit =
    let outPutPin = Espruino.NodeMCU.D2
    Espruino.digitalWrite (outPutPin, Espruino.OutPutStatus.Low)
    let wifi = (Espruino.require "Wifi" :?> Espruino.IWifi)


    let onConnected err =
        match err with
        | null -> 
            let write (header:string) (s:string) =
                Console.WriteLine header |> ignore
                Console.WriteLine s |> ignore
            Console.WriteLine ("IP: " + wifi.GetIP().ip)
            let http = (Espruino.require "http" :?> Espruino.IHttp)

            let callback : System.Func<_,_,_> =
                System.Func<_,_,_>(fun (request:IRequest) (response:IResponse) ->
                    let query = match  (Espruino.url.Parse request.url).Query with 
                    | null -> None
                    | a -> Some <| (a :?> IParameters)

                    response.WriteHead 200
                    
                    response.Write "<a href=\"?led=1\">on</a><br/><a href=\"?led=0\">off</a>"
                    response.End "sup dawg?"
                    match query with
                    | Some a ->
                        match a.Led with
                        | "1" -> digitalWrite (outPutPin, OutPutStatus.High)
                        | _ -> digitalWrite (outPutPin, OutPutStatus.Low)
                    | _ -> ()
                    ()
                )
            let server = http.CreateServer  callback
            server.Listen 80
        | _ -> Console.WriteLine ("Error: " + err)
    wifi.Connect (WifiSettings.ssid, WifiSettings.password, onConnected)

onInit ()