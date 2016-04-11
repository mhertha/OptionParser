
open System

#load "OptionParser.fs"
open OptionParser

// Test param function with long option
OptionParser.splitParam "--file fileName"
OptionParser.splitParam "--file:fileName"
OptionParser.splitParam "--file=fileName"

// Test param function with short option
OptionParser.splitParam "-f fileName"
OptionParser.splitParam "-f:fileName"
OptionParser.splitParam "-f=fileName"

// Get empty values
OptionParser.splitParam "-f"
OptionParser.splitParam "--file"

// These are no options
OptionParser.splitParam ""
OptionParser.splitParam "f"
OptionParser.splitParam "file"
OptionParser.splitParam "f:no-arg"
OptionParser.splitParam "file:no-arg"

type CommandOptions = {
        all : bool
        verbose : bool
        input : string
        output : string
        help : bool
    } with
    static member Default = {
        all = false; verbose = false; input = ""; output = ""; help = false
    }

let appOptions = [
      ParamOption ("-i", "--file FILENAME", "Die Eingabedatei" , (fun o s -> { o with input = s }))
      ParamOption ("-o", "--outdir OURDIR", "Das Ausgabeverzeichnis", (fun o s -> { o with output = s }))
      ParamOption ("", "--outvalue OUTVALUE", "Das Ausgabeverzeichnis2", (fun o s -> { o with output = s }))
      ParamOption ("", "--outvalue", "Das Ausgabeverzeichnis2", (fun o s -> { o with output = s }))
      ParamOption ("-oc OUTVALUE1", "--outvalue OUTVALUE2", "Das Ausgabeverzeichnis2", (fun o s -> { o with output = s }))
      ParamOption ("-od", "", "Das Ausgabeverzeichnis2", (fun o s -> { o with output = s }))
      ParamOption ("-of OUTVALUE", "", "Das Ausgabeverzeichnis2", (fun o s -> { o with output = s }))
      ParamOption ("", "", "Nur diese Informationen...", (fun o s -> { o with output = s }))
      SwitchOption ("-a", "--all", "Zeige alles an", (fun o b -> { o with all = b }))
      SwitchOption ("-a", "", "Zeige alles an2", (fun o b -> { o with all = b }))
      SwitchOption ("-av", "", "Zeige alles an2", (fun o b -> { o with all = b }))
      SwitchOption ("-avv", "", "Zeige alles an2", (fun o b -> { o with all = b }))
      SwitchOption ("-avvv", "", "Zeige alles an2", (fun o b -> { o with all = b }))
      SwitchOption ("-avvvv", "", "Zeige alles an2", (fun o b -> { o with all = b }))
      SwitchOption ("", "--allvalue", "Zeige alles an2", (fun o b -> { o with all = b }))
      SwitchOption ("-v", "--verbose", "Gebe zusätzliche Informationen aus", (fun o b -> { o with verbose = b }))
      SwitchOption ("-av", "--allvalue", "Zeige alles an2", (fun o b -> { o with all = b }))
      SwitchOption ("-h", "--help", "Zeige diese Hilfeinformation", (fun o b -> { o with help = b }))
      SwitchOption ("", "", "Zeige nur diese Hilfeinformation", (fun o b -> { o with help = b }))
    ]
let bannerText = "TestOptionParser.fsx [options]"
let separatorLines = [ ""; "Copyright © 2016 - mhitc.de"; "Optionen sind:" ]

let parser = OptionParser.create appOptions bannerText separatorLines           
parser.PrintUsage ()

let commandArgs = ["-a"; "-v"; "-i"; "filename.txt"; "-o"; "outdir"; "-h"]

let newOpts, _ = parser.Parse CommandOptions.Default commandArgs
printfn "%A" newOpts

let testOpts1, _ = parser.Parse CommandOptions.Default ["/a"; "/v"; "-i"; "filename.txt"; "-o"; "nextdir" ]
printfn "%A" testOpts1

let testOpts2, _ = parser.Parse CommandOptions.Default ["/a"; "/v"; "/file"; "filename.txt"; "--outvalue"; "nextdir" ]
printfn "%A" testOpts2

let testOpts3, _ = parser.Parse CommandOptions.Default ["/a"; "/v"; "/file:filename.txt"; "--outvalue:nextdir" ]
printfn "%A" testOpts3

let testOptsE, _ = parser.Parse CommandOptions.Default ["-a"; "-v"; "-i"; "filename.txt"; "-o"]
printfn "%A" testOptsE

let testOptsF, testArgsF = parser.Parse CommandOptions.Default ["-a"; "-v"; "-i"; "filename.txt"; "-o"; "outdir"; "--"; "file1"; "file2"]
printfn "%A -> %A" testOptsF testArgsF

// Error as parameter for -o is of type option sign!
let testOptsG, testArgsG = parser.Parse CommandOptions.Default ["-a"; "-v"; "-i"; "filename.txt"; "-o"; "--"; "file1"; "file2"]
printfn "%A -> %A" testOptsG testArgsG
