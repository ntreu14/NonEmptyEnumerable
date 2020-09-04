namespace NonEmptyEnumerable.Specs

module Main =
  open Expecto
  open Specs

  let testConfig = 
    { defaultConfig with runInParallel=true; verbosity=Logging.LogLevel.Verbose }

  [<EntryPoint>]
  let main argv =
    specs |> runTestsWithArgs testConfig argv 
