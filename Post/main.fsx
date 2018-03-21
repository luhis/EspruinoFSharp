#r "./node_modules/fable-core/Fable.Core.dll"
open System
#load "../espruino.fsx"
open Espruino
#load "../wifiSettings.fsx"

System.Console.WriteLine ("Initialising. Serial " + Espruino.getSerial ())

let mutable private state = Espruino.OutPutStatus.High

let private getOtherStatus s =
    match s with
    | Espruino.OutPutStatus.High -> Espruino.OutPutStatus.Low
    | _ -> Espruino.OutPutStatus.High

let private flash pin =
    let newStatus = getOtherStatus state
    Espruino.digitalWrite (pin, newStatus)
    state <- newStatus

let onInit () : unit =
    Espruino.digitalWrite (Espruino.NodeMCU.D2, Espruino.OutPutStatus.Low)
    let wifi = (Espruino.require "Wifi" :?> Espruino.IWifi)

    let onConnected err =
        match err with
        | null -> 
            let write (header:string) (s:string) =
                Console.WriteLine header |> ignore
                Console.WriteLine s |> ignore
            Console.WriteLine ("IP: " + wifi.GetIP().Ip)
            Espruino.setInterval ((fun () -> (flash Espruino.NodeMCU.D2)), 500) |> ignore
            let http = (Espruino.require "http" :?> Espruino.IHttp)
            // let onGot (res:Espruino.IHttpCRq) =
            //     res.On("data", write)
            // let get:Espruino.IHttpCRq = http.Get ("http://www.jibbering.com/2002/4/test.txt", onGot)
            // get.On("error", write)

            // let esp8266 = Espruino.require "ESP8266" :?> Espruino.IEsp8266

            // esp8266.Ping ("192.168.1.101", write "Ping response: ")

            let onPosted (res:Espruino.IHttpCRq) =
                res.On("data", write "data: ")
            let post:Espruino.IHttpCRq = http.Request ("https://httpbin.org", "POST", "/post", onPosted)
            post.On("error", write "error: ")
            post.Write "my content"
            post.End ()
        | _ -> Console.WriteLine ("Error: " + err)
    wifi.Connect (WifiSettings.ssid, WifiSettings.password, onConnected)

onInit ()