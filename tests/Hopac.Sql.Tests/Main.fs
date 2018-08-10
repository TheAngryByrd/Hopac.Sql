namespace Tests
open Expecto
open System
module Main =

    [<EntryPoint>]
    let main argv =
        let config =
            { defaultConfig with
                ``parallel`` = true
                parallelWorkers = System.Environment.ProcessorCount * 10}
        Tests.runTestsInAssembly config argv
