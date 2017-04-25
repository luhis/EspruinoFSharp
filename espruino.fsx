
#r "./node_modules/fable-core/Fable.Core.dll"
open Fable.Core

open System.Collections.Generic

    type Pin = Pin of int
    type OutPutStatus = High=1 | Low=0

    [<Erase>]
    [<Emit("getSerial()")>]
    let getSerial () : string = jsNative

    [<Erase>]
    [<Emit("get_system_time()")>]
    let getSystemTime () : int = jsNative

    type IntervalId = IntervalId of int

    [<Erase>]
    [<Emit("setInterval($0, $1)")>]
    let setInterval (func:(unit->unit), (time:int)) : IntervalId = jsNative

    
    [<Emit("clearInterval($0)")>]
    let clearInterval (id:IntervalId) : unit = jsNative

    
    [<Erase>]
    [<Emit("setTimeout($0, $1)")>]
    let SetTimeout (func:(unit->unit), (time:int)) : int = jsNative

    [<Erase>]
    [<Emit("digitalWrite($0, $1)")>]
    let digitalWrite (pin:Pin, state:OutPutStatus) : unit = jsNative

    [<Erase>]
    [<Emit("digitalPulse($0, $1, $2)")>]
    let digitalPulse (pin:Pin, state:OutPutStatus, time:int) : unit = jsNative

    [<Erase>]
    [<Emit("save()")>]
    let save () : unit = jsNative

    [<Erase>]
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

    [<Erase>]
    [<Emit("NodeMCU")>]
    let NodeMCU : INodeMCU = jsNative

    type IIpAddress =
        abstract member ip : string
        abstract member netmask : string
        abstract member gw : string
        abstract member mac : string

    type IWifi =
        [<Emit("$0.scan($1)")>]
        member x.Scan (f: (string -> unit)) :unit  = jsNative

        [<Emit("$0.connect($1, {password: $2}, $3)")>]
        member x.Connect ((accessPointName:string), (password:string), (onSuccess: string -> unit)) = jsNative

        [<Emit("$0.getIP()")>]
        member x.GetIP () : IIpAddress = jsNative

        [<Emit("$0.disconnect()")>]
        member x.Disconnect () : unit = jsNative


    type Options = {host:string; method:string}

    type IHttpCRq =
        [<Emit("$0.on($1, $2)")>]
        member x.On (evtName:string, f:string->unit) : unit = jsNative
        [<Emit("$0.end()")>]
        member x.End (contnet:string) : unit = jsNative

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
        abstract member url : string

    type IHttp =
        [<Emit("$0.request({host:$1, method:$2, path:$3}, $4)")>]
        member x.Request ((host:string), method:string, path:string, (cb: IHttpCRq -> unit)) : IHttpCRq = jsNative

        [<Emit("$0.get($1, $2)")>]
        member x.Get ((host:string), (cb: IHttpCRq -> unit)) : IHttpCRq = jsNative

        [<Emit("$0.createServer($1)")>]
        member x.CreateServer (callback:(System.Func<IRequest,IResponse,unit>)) : IHttpSrv = jsNative

    type IEsp8266 =
        [<Emit("$0.ping($1, $2)")>]
        member x.Ping (host:string, f:string -> unit) : unit = jsNative
