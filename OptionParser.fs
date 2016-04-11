(*
   Copyright 2016 - Maik Hertha

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
   -----------------------------------------------------------------------
   Module for processing command line arguments.
 *)
namespace OptionParser

open System

/// Union-Type for supported options to parse
type OptionType<'O> =
    | SwitchOption of string * string * string * ('O -> bool -> 'O)
    | ParamOption of string * string * string * ('O -> string -> 'O)

[<RequireQualifiedAccess>]
module OptionParser =
    
    open System.Text.RegularExpressions
    
    /// Helper function to define an regular expression
    let regex p = new Regex(p)

    /// Define a test function for so called short options aka strings
    /// with leading single dash not followed by a second dash or space char.
    let isShortOption =
        let rx = regex @"^-[^-\s]"
        fun s -> rx.IsMatch s
    /// Define a test function for so called long options aka strings
    /// with leading double dash not followed by an space char.
    let isLongOption =
        let rx = regex @"^--\S"
        fun s -> rx.IsMatch s
    /// Define a test function for options known on windows systems aka strings
    /// with leading single slash not followed by an space char.
    let isWinOption =
        let rx = regex @"^/\S"
        fun s -> rx.IsMatch s
    /// Define a test function for options ending marker to separate
    /// program options from a list of arguments
    let isOptionEnding =
        let rx = regex @"^--$"
        fun s -> rx.IsMatch s

    /// Test if option s is of any known option kind
    let isOption s =
        (isShortOption s) || (isLongOption s) || (isWinOption s) || (isOptionEnding s)

    /// Replace in source string the found pattern with replace string
    let replaceWith (pat : string) (rpl : string) (src : string) =
        (regex pat).Replace(src, rpl)
    /// Convert option sign double dash to slash
    let dashToSlash o =
        replaceWith @"^--?" "/" o
    /// Convert option sign slash to single dash
    let slashToDash o =
        replaceWith @"^/" "-" o
    /// Convert option sign slash to double dash
    let slashToDash2 o =
        replaceWith @"^/" "--" o

    /// Split values enclosed to it's parameter keys
    /// e.g. -n:2, --file=infile.txt
    let splitParam param =
        match isOption param with
        | false -> "", param
        | true ->
            let result = (regex @"^([^ :=]+)[ :=]?(.*)$").Match(param)
            match result.Success with
            | false -> param, ""
            | _ ->
                let groups = result.Groups
                match groups.Count with
                | 0 -> param, ""
                | _ -> groups.[1].Value, groups.[2].Value
    
    let buildSwitchLine s l t =
        match s, l with
        | "", "" -> sprintf "\t%-32s %s" " " t
        | x, "" ->
            let pad = if x.Length < 4 then 0 else 4 - x.Length
            sprintf "\t%-4s%s %s" x (String.replicate (28 + pad) " ") t
        | "", y -> sprintf "\t%-7s%-25s %s" " " y t
        | x, y -> sprintf "\t%-4s | %-25s %s" x y t

    let buildParamLine s l t =
        let oS, sP = splitParam s
        let oL, lP = splitParam l

        let mP =
            match sP, lP with
            | "", "" -> "ARGVALUE"
            | "", y -> y.ToUpper()
            | x, "" -> x.ToUpper()
            | x, y ->y.ToUpper()

        let mS, mL =
            match oS, oL with
            | "", "" -> "", ""
            | "", y -> "", (sprintf "%s %s" oL mP)
            | x, "" -> (sprintf "%s %s" x mP), ""
            | x, y -> x, (sprintf "%s %s" y mP)
        
        buildSwitchLine mS mL t

    let buildOptionLine = function
        | SwitchOption (s, l, t, _) -> buildSwitchLine s l t
        | ParamOption (s, l, t, _) -> buildParamLine s l t

    
    /// Union-Type to mark options to it's option sign
    type OptionMarker =
        | NoDash
        | SingleDash of string
        | DoubleDash of string
        | SingleSlash of string
    
    /// Helper function to define type of option kind
    let getOptionMarker o =
        match isShortOption o with
        | true -> SingleDash o
        | _ ->
            match isLongOption o with
            | true -> DoubleDash o
            | _ ->
                match isWinOption o with
                | true -> SingleSlash o
                | false -> NoDash

    (*
        For general information see referencing material on ruby web site
        See more: http://ruby-doc.org/stdlib-2.3.0/libdoc/optparse/rdoc/OptionParser.html 
     *)
    /// Generic OptionParser-Type
    type T<'O> =
        { options : OptionType<'O> list
          banner : string
          separator : string list }
        with
        member private m.makeBanner () =
            let banner =
                match m.banner with
                | "" -> "<appname> [options]"
                | b -> b
            let separator =
                let copyright = sprintf "Copyright © %d - Your Name Or Company" DateTime.Now.Year
                match m.separator with
                | [] -> [""; copyright; ""]
                | x -> x
            banner :: separator
            |> Seq.map (fun line -> line)

        member private m.makeUsageInfo () =
            match m.options with
            | [] -> seq { yield "No options authored!" }
            | x ->
                x
                |> Seq.map buildOptionLine

        member m.PrintUsage ?msg =
            let printLn line =
                printfn "%s" line
            
            match msg with
            | Some(m) ->
                printLn m
            | None ->
                ()

            Seq.append (m.makeBanner ()) (m.makeUsageInfo ())
            |> Seq.iter printLn
                
        member internal m.findSwitch = function
            | SingleDash o ->
                List.filter(function SwitchOption (oS, _, _, _) -> (slashToDash oS) = o | _ -> false) m.options
            | DoubleDash o ->
                List.filter (function SwitchOption (_, oL, _, _) -> (slashToDash2 oL) = o | _ -> false) m.options
            | SingleSlash o ->
                List.filter(function
                    | SwitchOption (oS, oL, _, _) -> ((dashToSlash oS) = o) || ((dashToSlash oL) = o)
                    | _ -> false) m.options
            | _ -> []

        member internal m.findParam = function
            | SingleDash o ->
                List.filter(function
                    | ParamOption (oS, _, _, _) ->
                        let pS, _ = splitParam oS
                        (slashToDash pS) = o
                     | _ -> false) m.options
            | DoubleDash o ->
                List.filter (function
                    | ParamOption (_, oL, _, _) ->
                        let pL, _ = splitParam oL
                        (slashToDash2 pL) = o
                    | _ -> false) m.options
            | SingleSlash o ->
                List.filter(function
                    | ParamOption (oS, oL, _, _) ->
                        let pS, _ = splitParam oS
                        let pL, _ = splitParam oL
                        ((dashToSlash pS) = o) || ((dashToSlash pL) = o)
                    | _ -> false) m.options
            | _ -> []

        /// Parser function for given command line options and update user
        /// defined option type.
        member m.Parse opts cmds =
            let rec parseFn (opts, args : string list) cmds =
                match cmds with
                | [] -> opts, args
                | "--" :: xs -> opts, args @ xs
                | x :: xs ->
                    let paramKey, enclosedValue = splitParam x
                    let pmt = getOptionMarker paramKey
                    match m.findSwitch pmt with
                    | [SwitchOption (_, _, _, fn)] ->
                        parseFn ((fn opts true), args) xs
                    | _ ->
                        match m.findParam pmt with
                        | [ParamOption (_, _, _, fn)] ->
                            let nextParams =
                                match String.IsNullOrEmpty enclosedValue with
                                | true -> xs
                                | false -> enclosedValue :: xs

                            match nextParams with
                            | y :: ys ->
                                match isOption y with
                                | false -> 
                                    parseFn ((fn opts y), args) ys
                                | true ->
                                    eprintfn "Parameter might not an option! '%s' " y
                                    parseFn (opts, args) ys
                            | [] -> failwithf "Option requires an argument '%s'" x
                        | _ ->
                            eprintfn "Unknown Option '%s'" x
                            parseFn (opts, args) xs

            parseFn (opts, []) cmds
        
        static member Create (opts, ?bannerText, ?separatorLines) =
            let banner' = defaultArg bannerText ""
            let lines' = defaultArg separatorLines []
            { options = opts; banner = banner'; separator = lines' }

    /// Create a new OptionParser-Type
    let create (newOpts : OptionType<'O> list) (newBanner : string) (newSeparator : string list) =
        T<'O>.Create (newOpts, newBanner, newSeparator)
