
#r "./node_modules/fable-core/Fable.Core.dll"
#r "./node_modules/fable-compiler/bin/Fable.Compiler.dll"
open Fable.Core
open Fable.AST
open Fable.Plugins

open System.Collections.Generic

    type Pin = Pin of int
    type OutPutStatus = High=1 | Low=0

    [<Emit("getSerial()")>]
    let getSerial () : string = jsNative

    [<Emit("get_system_time()")>]
    let getSystemTime () : int = jsNative

    type IntervalId = IntervalId of int

    [<Emit("setInterval($0, $1)")>]
    let setInterval (func:(unit->unit), (time:int)) : IntervalId = jsNative

    
    [<Emit("clearInterval($0)")>]
    let clearInterval (id:IntervalId) : unit = jsNative

    [<Emit("setTimeout($0, $1)")>]
    let SetTimeout (func:(unit->unit), (time:int)) : int = jsNative

    [<Emit("digitalWrite($0, $1)")>]
    let digitalWrite (pin:Pin, state:OutPutStatus) : unit = jsNative
        // let intConst (x:int) =
        //     Fable.NumberConst (float x, Int32) |> Fable.Value
        // let emitExpr =
        //     Fable.Emit("digitalWrite($0, $1)")
        //     |> Fable.Value
        // let (Pin p) = pin 
        // Fable.Apply(emitExpr, [intConst <| p; intConst <| int state], Fable.ApplyMeth, Fable.Type.Unit, None)

    //[<Erase>]
    [<Emit("digitalPulse($0, $1, $2)")>]
    let digitalPulse (pin:Pin, state:OutPutStatus, time:int) : unit = jsNative

    [<Emit("save()")>]
    let save () : unit = jsNative

    [<Emit("require($0)")>]
    let require (s:string) : obj = jsNative

    type IParseResult =
        [<Emit("$0.query")>]
        member x.Query : obj = jsNative

    type url =
        [<Emit("url.parse($0, true)")>]
        static member Parse (url:string) : IParseResult = jsNative
 
    type INodeMCU =
        abstract A1 : Pin
        abstract D1 : Pin
        abstract D2 : Pin
        abstract D3 : Pin
        abstract D4 : Pin
        abstract D5 : Pin
        abstract D6 : Pin
        abstract D7 : Pin
        abstract D8 : Pin

    [<Emit("NodeMCU")>]
    let NodeMCU : INodeMCU = jsNative

    type ISpiPort =
        [<Emit("$0.send($1, $2)")>]
        member x.Send (data:byte[], nssPin:Pin) = jsNative

    type ISpi =
        [<Emit("$0.find($1)")>]
        member x.Find (pin:Pin) : ISpiPort = jsNative

    let SPI : ISpi = jsNative

    type II2CPort =
        [<Emit("$0.readFrom($1, $2")>]
        member x.ReadFrom (address:uint8, quantity:int) = jsNative
        [<Emit("$0.writeTo($1, $2")>]
        member x.WriteTo (address:uint8, data:byte[]) = jsNative

    type II2C =
        [<Emit("$0.find($1)")>]
        member x.Find (pin:Pin) : II2CPort = jsNative
    let I2C : II2C = jsNative

    type IIpAddress =
        [<Emit("$0.ip")>]
        abstract member Ip : string
        [<Emit("$0.netmask")>]
        abstract member Netmask : string
        [<Emit("$0.gw")>]
        abstract member Gw : string
        [<Emit("$0.mac")>]
        abstract member Mac : string

    type IWifi =
        [<Emit("$0.scan($1)")>]
        member x.Scan (f: (string -> unit)) :unit  = jsNative

        [<Emit("$0.connect($1, {password: $2}, $3)")>]
        member x.Connect ((accessPointName:string), (password:string), (onSuccess: string -> unit)) : unit = jsNative

        [<Emit("$0.getIP()")>]
        member x.GetIP () : IIpAddress = jsNative

        [<Emit("$0.disconnect()")>]
        member x.Disconnect () : unit = jsNative


    type Options = {host:string; method:string}

    type IHttpCRq =
        [<Emit("$0.on($1, $2)")>]
        member x.On (evtName:string, f:string->unit) : unit = jsNative
        [<Emit("$0.end()")>]
        member x.End () : unit = jsNative

        [<Emit("$0.write($1)")>]
        member c.Write (content:string) : unit = jsNative

    type IHttpSrv =
        [<Emit("$0.close()")>]
        member x.Close () : unit = jsNative

        [<Emit("$0.listen($1)")>]
        member x.Listen (port:int) : unit = jsNative

    type IResponse =
        [<Emit("$0.writeHead($1)")>]
        member x.WriteHead (status:int) = jsNative

        [<Emit("$0.write($1)")>]
        member s.Write string : unit = jsNative

        [<Emit("$0.end($1)")>]
        member s.End string : unit = jsNative

    type IRequest =
        [<Emit("$0.url")>]
        abstract member Url : string

    type IHttp =
        [<Emit("$0.request({host:$1, method:$2, path:$3}, $4)")>]
        member x.Request ((host:string), method:string, path:string, (cb: IHttpCRq -> unit)) : IHttpCRq = jsNative

        [<Emit("$0.get($1, $2)")>]
        member x.Get ((host:string), (cb: IHttpCRq -> unit)) : IHttpCRq = jsNative

        [<Emit("$0.createServer($1)")>]
        member x.CreateServer (callback:(System.Func<IRequest,IResponse,unit>)) : IHttpSrv = jsNative

    type IEsp8266 =
        [<Emit("$0.ping($1, $2)")>]
        member x.Ping (host:string, callBack:string -> unit) : unit = jsNative

    type IMqttPublishMessage =
        [<Emit("$0.topic")>]
        abstract member Topic : string
        [<Emit("$0.message")>]
        abstract member Message : string

    [<StringEnum>]
    type IEvent = Connected | Publish | Error | Subscribed

    type IMqttConnection =
        [<Emit("$0.on($1, $2)")>]
        member x.On (evtName:IEvent, f:obj->unit) : unit = jsNative
        member x.OnPublish (f:IMqttPublishMessage->unit) : unit = x.On (Publish, (fun o -> o :?> IMqttPublishMessage |> f))

        [<Emit("$0.disconnect()")>]
        member x.Disconnect () : unit = jsNative
        [<Emit("$0.subscribe($1)")>]
        member x.Subscribe (s:string) : unit = jsNative
        [<Emit("$0.publish($1, $2)")>]
        member x.Publish (topic:string, message:string) : unit = jsNative

        [<Emit("$0.connect()")>]
        member x.Connect () : unit = jsNative

    type IMqtt =
        [<Emit("$0.create($1)")>]
        member x.Create (ip:string) : IMqttConnection = jsNative

    let Mqtt : IMqtt = require "mqtt" :?> IMqtt