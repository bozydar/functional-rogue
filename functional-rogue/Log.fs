module Log 

open System
open System.IO

type LogType = 
    | Trace    
    | Info
    | Warn
    | Error

let private filePath = "Log.log"
let private openFile() = 
    File.AppendText(filePath)

let private formatMessage (logType: LogType) (message: string) (ex: option<Exception>) =     
    "[" + repr logType + "] " +  DateTime.Now.ToLongTimeString() + ": " + message + (if ex.IsSome then ": " + ex.Value.ToString() else String.Empty) + "\n"

let logException logType message ex = 
    use strm = openFile()
    let message = formatMessage logType message ex
    strm.Write(message)    

let log logType message =
    use strm = openFile()
    let message = formatMessage logType message option.None
    strm.Write(message)
    