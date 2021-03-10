module Main 

open Expecto

[<EntryPoint>]
let main =
  runTestsInAssemblyWithCLIArgs
    [ FsCheck_Max_Tests 1000
      Parallel
      Verbosity Logging.LogLevel.Verbose
    ]