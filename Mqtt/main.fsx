// #r "./node_modules/fable-core/Fable.Core.dll"
open System
#load "../espruino.fsx"
open Espruino
#load "../wifiSettings.fsx"

let onInit () =
    Espruino.digitalWrite (Espruino.NodeMCU.D2, Espruino.OutPutStatus.Low)
    let wifi = (Espruino.require "Wifi" :?> Espruino.IWifi)
    let onConnected err =
        match err with
        | null -> 
            Console.WriteLine "Wifi Connected"
            let mqtt = (Espruino.require "MQTT" :?> Espruino.IMqtt).Create "192.168.1.104"
            mqtt.On (Connected, 
                fun _ -> 
                    Console.WriteLine "MQTT Connected"
                    mqtt.Subscribe "fsharp"
                )
            mqtt.On (Publish, fun data -> 
                let a = data :?> IMqttPublishMessage
                Console.WriteLine a)
            mqtt.Connect ()
        | _ -> Console.WriteLine ("Wifi Error: " + err)
    wifi.Connect (WifiSettings.ssid, WifiSettings.password, onConnected)

onInit ()