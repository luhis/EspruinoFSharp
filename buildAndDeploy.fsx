let build = new System.Diagnostics.Process ()
build.StartInfo.FileName <- "node"
build.StartInfo.Arguments <- @"..\node_modules\fable-compiler\ .\main.fsx"
build.StartInfo.UseShellExecute <- false

build.Start ()
build.WaitForExit ()

match build.ExitCode with
| 0 -> 
    let firstPort = System.IO.Ports.SerialPort.GetPortNames() |> Seq.head
    let portName = firstPort
    let baud = 115200
    System.Console.WriteLine "Build Success"
    let script = System.IO.File.ReadAllText "./out/bundle.js"
    use port = new System.IO.Ports.SerialPort (portName, baud, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One)
    port.DtrEnable <- true
    port.RtsEnable <- true
    port.Open ()
    port.WriteLine("reset();")
    port.ReadLine () |> ignore
    port.WriteLine script
    port.BaseStream.Flush ()

    port.ReadExisting () |> ignore

    System.Console.WriteLine "Hit ctrl C to quit"
    while true do
        System.Threading.Thread.Sleep 500
        System.Console.Write ( port.ReadExisting ())
    
| _ -> System.Console.WriteLine "Build Fail"
