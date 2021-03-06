namespace Tests


module ExpectoHelpers =
    open Expecto
    open Hopac

    type ParameterizedTest<'a> =
    | Sync of string * ('a -> unit)
    | Async of string * ('a -> Async<unit>)
    | Job of string * ('a -> Job<unit>)


    let testCase' name test =
         ParameterizedTest.Sync(name,test)

    let testCaseAsync' name test  =
        ParameterizedTest.Async(name,test)

    let testCaseJob' name test  =
        ParameterizedTest.Job(name,test)

    let inline testFixtureAsync<'a> (setup) =
        Seq.map (fun ( parameterizedTest : ParameterizedTest<'a>) ->
            match parameterizedTest with
            | Sync (name, test) ->
                testCase name <| fun () -> test >> Job.result |> setup |> run
            | Async (name, test) ->
                testCaseAsync name <| (setup (test >> Job.fromAsync) |> Job.toAsync)
            | Job(name,test) ->
                testCaseJob name <| setup test
        )
