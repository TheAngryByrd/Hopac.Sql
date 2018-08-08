module Tests


open Expecto
open Hopac.Sql

[<Tests>]
let tests =
  testList "samples" [
    testCase "Say nothing" <| fun _ -> ()

  ]
