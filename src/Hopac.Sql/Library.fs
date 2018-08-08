namespace Hopac.Sql

open System.Data.Common
open Hopac

[<AutoOpen>]
module Infixes =
    let (^) = (<|)

[<RequireQualifiedAccess>]
module DbConnection =
    let openConn (conn : #DbConnection) =
        Alt.fromUnitTask conn.OpenAsync

[<RequireQualifiedAccess>]
module DbCommand =
    let executeReader behavior (cmd : #DbCommand) =
        Alt.fromTask ^
            fun ct -> cmd.ExecuteReaderAsync(behavior,ct)

    let executeNonQuery (cmd : #DbCommand) =
        Alt.fromTask cmd.ExecuteNonQueryAsync

    let executeScalar<'a> (cmd : DbCommand) =
        Alt.fromTask cmd.ExecuteScalarAsync
        |> Alt.afterFun unbox<'a>


[<RequireQualifiedAccess>]
module DbDataReader =
    let read (reader : #DbDataReader) =
        Alt.fromTask reader.ReadAsync

    let nextResult (reader : #DbDataReader) =
        Alt.fromTask reader.NextResultAsync

    // https://blogs.msdn.microsoft.com/adonet/2012/04/20/using-sqldatareaders-new-async-methods-in-net-4-5/
    let getFieldValue index (reader : #DbDataReader) =
        Alt.fromTask ^
            fun ct -> reader.GetFieldValueAsync(index,ct)

    // https://blogs.msdn.microsoft.com/adonet/2012/04/20/using-sqldatareaders-new-async-methods-in-net-4-5/
    let isDBNull index (reader : #DbDataReader) =
        Alt.fromTask ^
            fun ct -> reader.IsDBNullAsync(index,ct)


module Extensions =

    type System.Data.Common.DbConnection with
        member x.OpenJob () =
            DbConnection.openConn x


    type System.Data.Common.DbCommand with
        member this.ExecuteReaderJob (behavior) =
            DbCommand.executeReader behavior this
        member this.ExecuteNonQueryJob () =
            DbCommand.executeNonQuery this
        member this.ExecuteScalarJob () =
            DbCommand.executeScalar this

    type System.Data.Common.DbDataReader with
        member this.ReadJob () =
            DbDataReader.read this
        member this.GetFieldValueJob (index) =
            DbDataReader.getFieldValue index this
        member this.IsDBNullJob (index) =
            DbDataReader.isDBNull index this
