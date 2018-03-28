let build = new System.Diagnostics.Process ()
build.StartInfo.FileName <- "node"
build.StartInfo.Arguments <- @"..\node_modules\fable-compiler\ .\main.fsx"
build.StartInfo.UseShellExecute <- false

build.Start ()
build.WaitForExit ()

match build.ExitCode with
| 0 -> 
    System.Console.WriteLine "Build Success"
    let firstPort = System.IO.Ports.SerialPort.GetPortNames() |> Seq.tryHead
    match firstPort with
    | Some port -> 
        let baud = 115200
        let script = System.IO.File.ReadAllText "./out/bundle.js"
        use port = new System.IO.Ports.SerialPort (port, baud, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One)
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
            match port.ReadExisting () with
            | "" -> ()
            | s -> System.Console.WriteLine s
    | None -> System.Console.WriteLine "No serial port found"
| _ -> System.Console.WriteLine "Build Fail"
