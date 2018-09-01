namespace Hopac.Sql

open Hopac
open System.Data
module Reflection =
    open System
    open System.Data.Common
    open Microsoft.FSharp.Reflection
    let toOptionDynamic (typ: Type) (value: obj) =
        let opttyp = typedefof<Option<_>>.MakeGenericType([|typ|])
        let tag, varr = if DBNull.Value.Equals(value) then 0, [||] else 1, [|value|]
        let case = FSharpType.GetUnionCases(opttyp) |> Seq.find (fun uc -> uc.Tag = tag)
        FSharpValue.MakeUnion(case, varr)

    let optionTypeArg (typ : Type) =
        let isOp = typ.IsGenericType && typ.GetGenericTypeDefinition() = typedefof<Option<_>>
        if isOp then Some (typ.GetGenericArguments().[0]) else None

    let getFieldsAndType<'R> =
        let rty = typeof<'R>
        assert (FSharpType.IsRecord(rty))
        FSharpType.GetRecordFields(rty)
        |> Array.map(fun pi -> (pi.Name, pi.PropertyType))

    let getFieldsAndValue<'R> (record : 'R) =
        let rty = typeof<'R>
        assert (FSharpType.IsRecord(rty))
        FSharpType.GetRecordFields(rty)
        |> Array.map(fun pi -> (pi.Name ,pi.GetValue(record)))

    let makeEntity rty vals =
        FSharpValue.MakeRecord(rty, vals) :?> 'R
    let makeRecord<'a> fields rty  (reader : DbDataReader) : 'a=
        fields
        |> Array.map(fun (name,type2) ->
            let value =
                reader.GetOrdinal name
                |> reader.GetValue
            match optionTypeArg type2 with
            | Some t -> toOptionDynamic t value
            | None -> value
        )
        |> makeEntity rty

module Sql =
    open Hopac
    open Hopac.Infixes
    open System
    open System.Data.Common
    let toRecordStream<'a> (nack : Promise<unit>) (reader : DbDataReader) =
        let fields = Reflection.getFieldsAndType<'a>
        let rty = typeof<'a>

        let handleRead canRead =
            if canRead then
                let record =
                    reader
                    |> Reflection.makeRecord<'a> fields rty
                Some(record, ())
            else
                None

        let unfolder () = job {
            return!
                (reader |> DbDataReader.read) ^-> handleRead
                <|> nack ^->. None
        }
        Stream.unfoldJob unfolder ()

    let toRecordList<'a> (nack : Promise<unit>) (reader : DbDataReader) : Job<ResizeArray<'a>>=
        toRecordStream nack reader
        |> Stream.toSeq


    let (|SomeObj|_|) =
      let ty = typedefof<option<_>>
      fun (a:obj) ->
        let aty = a.GetType()
        if aty.IsGenericType && aty.GetGenericTypeDefinition() = ty then
          let v = aty.GetProperty("Value")
          if a = null then None
          else Some(v.GetValue(a, [| |]))
        else None

    let rec normalizeParameters (key,value) =
        match value with
        | null ->
            (key,box DBNull.Value)
        | SomeObj(x) ->
            normalizeParameters (key,x)
        | _ ->
            (key, value)

    let queryList<'a> (conn : DbConnection) (sql : string) (parameters : (string * obj) array) =
        Alt.withNackJob2 ^ fun nack ->
            job {
                use cmd = conn.CreateCommand(CommandText = sql)
                parameters
                |> Array.map(normalizeParameters)
                |> Array.iter(fun (key,value) ->
                    let p = cmd.CreateParameter(ParameterName=key, Value = value)
                    cmd.Parameters.Add p |> ignore
                )
                do! DbConnection.ensureOpen conn
                    <|> nack
                let readCommand =
                    cmd |> DbCommand.executeReader CommandBehavior.Default
                        |> Alt.afterJob(fun reader -> job{
                            use reader = reader
                            return! reader |> toRecordList<'a> nack
                        })
                return!
                    readCommand
                    <|> Alt.nackWithDefault<_> nack
            }

    let queryScalar<'a> (conn : DbConnection) (sql : string) (parameters : (string * obj) array) =
        Alt.withNackJob2 ^ fun nack ->
            job {
                use cmd = conn.CreateCommand(CommandText = sql)
                parameters
                |> Array.map(normalizeParameters)
                |> Array.iter(fun (key,value) ->
                    let p = cmd.CreateParameter(ParameterName=key, Value = value)
                    cmd.Parameters.Add p |> ignore
                )
                do! DbConnection.ensureOpen conn
                    <|> nack
                let readCommand =
                    cmd |> DbCommand.executeScalar<'a>
                return!
                    readCommand
                    <|> Alt.nackWithDefault<_> nack
            }

    let simpleInsert table (records : 'a array) (conn : DbConnection) =
        Alt.withNackJob2 ^ fun nack ->
            job {
                let fields = records |> Array.map Reflection.getFieldsAndValue<'a>

                let generateColumns fields =
                    let field = fields |> Seq.head
                    field
                    |> Array.map(fun (name,_) ->
                        name
                    )
                    |> String.concat ","
                    |> sprintf "(%s)"

                let columns = generateColumns fields
                let generateValueParameters (fields : array<(string * obj) []>) =
                    fields
                    |> Array.mapi(fun index field ->
                        let values, parameters =
                            field
                            |> Array.map(fun ((name,value)) ->
                                let paramName = (sprintf "@%s%d" name index ).ToLower()
                                (paramName, normalizeParameters(paramName,value))
                            )
                            |> Array.unzip
                        let value =
                            values
                            |> String.concat ","
                        (sprintf "(%s)" value, parameters)
                     )
                   |> Array.unzip
                   |> fun (f,s) -> f, s |> Array.collect id

                let values, parameters = generateValueParameters fields

                let value = values |> String.concat ","

                let sql = sprintf "INSERT INTO %s %s VALUES %s" table columns value
                use cmd = conn.CreateCommand(CommandText = sql)
                parameters
                |> Array.iter(fun (key,value) ->
                    let p = cmd.CreateParameter(ParameterName=key, Value = value)
                    cmd.Parameters.Add p |> ignore
                )

                do! DbConnection.ensureOpen conn
                    <|> nack
                return!
                    DbCommand.executeNonQuery cmd
                    <|> Alt.nackWithDefault<_> nack
             }
