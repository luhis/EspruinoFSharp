#r "./node_modules/fable-core/Fable.Core.dll"
open Fable.Core
open System
#load "../espruino.fsx"
open Espruino
#load "../wifiSettings.fsx"

System.Console.WriteLine ("Initialising. Serial " + Espruino.getSerial ())

[<Erase>]
type IParameters =
    [<Emit("$0.led")>]
    member x.Led : string = jsNative

let onInit () =
    let outPutPin = Espruino.NodeMCU.D2
    Espruino.digitalWrite (outPutPin, Espruino.OutPutStatus.Low)
    let wifi = (Espruino.require "Wifi" :?> Espruino.IWifi)

    let onConnected = function
        | null ->
            Console.WriteLine ("IP: " + wifi.GetIP().Ip)
            let http = (Espruino.require "http" :?> Espruino.IHttp)

            let callback =
                System.Func<_,_,_>(fun (request:IRequest) (response:IResponse) ->
                    let query = match  (Espruino.url.Parse request.Url).Query with 
                                | null -> None
                                | a -> Some (a :?> IParameters)

                    response.WriteHead 200
                    
                    response.End "<a href=\"?led=1\">on</a><br/><a href=\"?led=0\">off</a>"
                    match query with
                    | Some q when q.Led = "1" -> digitalWrite (outPutPin, OutPutStatus.High)
                    | Some _ -> digitalWrite (outPutPin, OutPutStatus.Low)
                    | _ -> ()
                )
            let server = http.CreateServer callback
            server.Listen 80
        | err -> Console.WriteLine ("Error: " + err)
    wifi.Connect (WifiSettings.ssid, WifiSettings.password, onConnected)

onInit ()