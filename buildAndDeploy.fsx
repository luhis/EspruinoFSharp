let build = new System.Diagnostics.Process ()
build.StartInfo.FileName <- "node"
build.StartInfo.Arguments <- @"..\node_modules\fable-compiler\ .\main.fsx"
build.StartInfo.UseShellExecute <- false

build.Start ()
build.WaitForExit ()

match build.ExitCode with
| 0 -> 
    let portName = "COM3"
    let baud = 115200
    System.Console.WriteLine "Build Success"
    let script = System.IO.File.ReadAllText "./out/bundle.js"
    use port = new System.IO.Ports.SerialPort (portName, baud)
    port.Open ()
    port.WriteLine script
    port.BaseStream.Flush ()
    //port.DiscardInBuffer ()
    //port.Close ()

    port.ReadExisting () |> ignore
    //System.Threading.Thread.Sleep 100

    
    System.Console.WriteLine "Hit ctrl C to quit"
    // use listenPort = new System.IO.Ports.SerialPort (portName, baud)
    // listenPort.Open ()
    // listenPort.Close ()
    while true do
        System.Threading.Thread.Sleep 500
        System.Console.Write ( port.ReadExisting ())
    
| _ -> 
    System.Console.WriteLine "Build Fail"