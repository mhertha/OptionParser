Readme
======

The project provides an OptionParser module for processing command line arguments in your F# application.
The aim is to minimize the effort for definition and processing for the options.

The OptionParser class from the Ruby language was an example for this.

How Is The Code Organized
-------------------------
The code has its own namespace OptionParser. That gives access to types and an OptionParser module which
requires qualified access.

Which Option Types Exists
-------------------------
There are two supported option types
* an option that behaves as switch and
* an option that sets values.

Both options have two flavors
* short option defined with a single dash and one to two chars (char or digit)
* long option defined with double dashes and an descriptive word

A third flavor is on windows systems. There are also short and long options possible but not marked with
single or double dashes but rather with a single slash.

All three flavors to sign an option are supported.

Quick Start
-----------
Add this module to your project and open the namespace.

Define your option record type with a static function for setting the defaults.

Then define your command line options as list, set a banner line and separator lines between banner
and options list. The option list is created on the fly.

The options you define are of union type with the same set of values.

    SwitchOption of string * string * string * ('O -> bool -> 'O)
e.g.

    SwitchOption ("-s", "--switch-option", "description for <switch-option>", <record update function>)

Simply the same style for option with value.

    ParamOption of string * string * string * ('O -> bool -> 'O)
e.g.

    ParamOption ("-l", "--long-option VALUE", "description for <long-option>", <record update function>)

If the value name is ommitted the PrintUsage function will generate an descriptive name for you.

As the update function is under control of the developer I believe there is no need for ParamOptions
of a specific value type (e.g. IntvalOption, DateTimeOption etc.)

After all is setup call the Parse function with defaults of your option record type and the command arguments.
If the argv array is used convert it to a list type.
    
    open OptionParser

    // supported command line arguments
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

    // list of options
    let myOpts = [
      ParamOption ("-i", "--file FILENAME", "Input file name" , (fun o s -> { o with input = s }))
      ParamOption ("-o", "--outdir OURDIR", "Output directory", (fun o s -> { o with output = s }))
      SwitchOption ("", "--all", "Show all results", (fun o b -> { o with all = b }))
      SwitchOption ("-v", "--verbose", "Additional processing information", (fun o b -> { o with verbose = b }))
      SwitchOption ("-h", "--help", "Show this help page", (fun o b -> { o with help = b }))
    ]
    let myBanner = "<yourapp.exe> [options]"
    let mySeps = [""; "Mostly the copyright notice"; "Options are:"]
    
    // Module functions are not allowed for optional parameters, so all have to set
    let parser = OptionParser.create myOpts myBanner mySeps
    
    // Test the usage information
    parser.PrintUsage ()
    
    // Parse the command line 
    let opts, args = parser.Parse CommandOptions.Default (List.ofArray argv)
    
    // Test case
    let commandLine = ["-a"; "-v"; "-i"; "filename.txt"; "-o"; "outdir"; "-h"]
    let opts, args = parser.Parse CommandOptions.Default commandLine

The variable opts has your updated record type and the args list has all values which are found behind a double
dash. This is common to sign the end of command options.