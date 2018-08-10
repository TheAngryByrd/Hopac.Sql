namespace Hopac.Sql

open Hopac
[<AutoOpen>]
module Infixes =
    let (^) = (<|)

module Hopac =
    open Hopac
    open Hopac.Infixes
    module Alt =
        let withNackJob2 j =
            Alt.withNackJob ^ fun nack ->
                Promise.start ^ j nack

        let nackWithDefault<'a> (nack : Alt<unit>) =
            nack
            ^->. Unchecked.defaultof<'a>
