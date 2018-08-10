namespace Hopac.Sql

open System.Data.Common
open Hopac
open System.Data

[<RequireQualifiedAccess>]
module DbConnection =

    /// **Description**
    ///
    /// An `Alt` version of `OpenAsync`
    ///
    /// **Upstream**
    ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
    ///   * `OpenAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbconnection.openasync?view=netstandard-2.0#System_Data_Common_DbConnection_OpenAsync_System_Threading_CancellationToken_
    ///
    /// **Parameters**
    ///   * `conn` - parameter of type `DbConnection`
    ///
    /// **Output Type**
    ///   * `Alt<unit>`
    let openConnection (conn : DbConnection) =
        Alt.fromUnitTask conn.OpenAsync


    /// **Description**
    ///
    /// If a connection's ConnectionState is Closed, this opens the connection.
    ///
    /// **Parameters**
    ///   * `conn` - parameter of type `DbConnection`
    ///
    /// **Output Type**
    ///   * `Alt<unit>`
    let ensureOpen (conn : DbConnection) =
         if conn.State = ConnectionState.Closed then
            openConnection conn
         else
            Alt.unit ()


[<RequireQualifiedAccess>]
module DbCommand =

    /// **Description**
    ///
    /// An `Alt` version of `ExecuteReaderAsync`
    ///
    /// **Upstream**
    ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
    ///   * `ExecuteReaderAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbcommand.executereaderasync?view=netstandard-2.0#System_Data_Common_DbCommand_ExecuteReaderAsync_System_Data_CommandBehavior_System_Threading_CancellationToken_
    ///
    /// **Parameters**
    ///   * `behavior` - parameter of type `System.Data.CommandBehavior`
    ///   * `cmd` - parameter of type `DbCommand`
    ///
    /// **Output Type**
    ///   * `Alt<DbDataReader>`
    let executeReader (behavior : CommandBehavior) (cmd : DbCommand) =
        Alt.fromTask ^
            fun ct -> cmd.ExecuteReaderAsync(behavior,ct)


    /// **Description**
    /// An `Alt` version of `ExecuteNonQueryAsync`
    ///
    /// **Upstream**
    ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
    ///   * `ExecuteNonQueryAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbcommand.executenonqueryasync?view=netstandard-2.0#System_Data_Common_DbCommand_ExecuteNonQueryAsync_System_Threading_CancellationToken_
    ///
    /// **Parameters**
    ///
    ///   * `cmd` - parameter of type `DbCommand`
    ///
    /// **Output Type**
    ///   * `Alt<int>`
    let executeNonQuery (cmd : DbCommand) =
        Alt.fromTask cmd.ExecuteNonQueryAsync


    /// **Description**
    ///
    /// An `Alt` version of `ExecuteScalarAsync`
    ///
    /// **Upstream**
    ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
    ///   * `ExecuteScalarAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbcommand.executescalarasync?view=netstandard-2.0#System_Data_Common_DbCommand_ExecuteScalarAsync_System_Threading_CancellationToken_
    ///
    /// **Parameters**
    ///
    ///   * `cmd` - parameter of type `DbCommand`
    ///
    /// **Output Type**
    ///   * `Alt<'a>`
    let executeScalar<'a> (cmd : DbCommand) =
        Alt.fromTask cmd.ExecuteScalarAsync
        |> Alt.afterFun unbox<'a>


[<RequireQualifiedAccess>]
module DbDataReader =

    /// **Description**
    ///
    /// An `Alt` version of `ReadAsync`
    ///
    /// **Upstream**
    ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
    ///   * `ReadAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbdatareader.readasync?view=netstandard-2.0#System_Data_Common_DbDataReader_ReadAsync_System_Threading_CancellationToken_
    ///
    /// **Parameters**
    ///   * `reader` - parameter of type `DbDataReader`
    ///
    /// **Output Type**
    ///   * `Alt<bool>`
    let read (reader : DbDataReader) =
        Alt.fromTask reader.ReadAsync

    /// **Description**
    ///
    /// An `Alt` version of `NextResultAsync`
    ///
    /// **Upstream**
    ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
    ///   * `NextResultAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbdatareader.nextresultasync?view=netstandard-2.0#System_Data_Common_DbDataReader_NextResultAsync_System_Threading_CancellationToken_
    ///
    /// **Parameters**
    ///   * `reader` - parameter of type `DbDataReader`
    ///
    /// **Output Type**
    ///   * `Alt<bool>`
    let nextResult (reader : DbDataReader) =
        Alt.fromTask reader.NextResultAsync

    /// **Description**
    ///
    /// An `Alt` version of `GetFieldValueAsync`
    ///
    /// **Upstream**
    ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
    ///   * `GetFieldValueAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbdatareader.getfieldvalueasync?view=netstandard-2.0#System_Data_Common_DbDataReader_GetFieldValueAsync__1_System_Int32_System_Threading_CancellationToken_
    ///
    /// **WARNING**
    ///
    /// You only want to use this when using CommandBehavior.SequentialAccess. See: https://blogs.msdn.microsoft.com/adonet/2012/04/20/using-sqldatareaders-new-async-methods-in-net-4-5/
    ///
    /// **Parameters**
    ///   * `ordinal` - parameter of type `int` - The zero-based column to be retrieved.
    ///   * `reader` - parameter of type `DbDataReader`
    ///
    /// **Output Type**
    ///   * `Alt<'a>`
    let getFieldValue (ordinal : int) (reader : DbDataReader) =
        Alt.fromTask ^
            fun ct -> reader.GetFieldValueAsync(ordinal,ct)

    /// **Description**
    ///
    /// An `Alt` version of `IsDBNullAsync`
    ///
    /// **Upstream**
    ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
    ///   * `IsDBNullAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbdatareader.isdbnullasync?view=netstandard-2.0#System_Data_Common_DbDataReader_IsDBNullAsync_System_Int32_System_Threading_CancellationToken_
    ///
    /// **WARNING**
    ///
    /// You only want to use this when using CommandBehavior.SequentialAccess. See: https://blogs.msdn.microsoft.com/adonet/2012/04/20/using-sqldatareaders-new-async-methods-in-net-4-5/
    ///
    /// **Parameters**
    ///   * `ordinal` - parameter of type `int` - The zero-based column to be retrieved.
    ///   * `reader` - parameter of type `DbDataReader`
    ///
    /// **Output Type**
    ///   * `Alt<bool>`
    let isDBNull (ordinal : int) (reader : DbDataReader) =
        Alt.fromTask ^
            fun ct -> reader.IsDBNullAsync(ordinal,ct)


module Extensions =
    type System.Data.Common.DbConnection with

        /// **Description**
        /// An `Alt` version of `OpenAsync`
        ///
        /// **Upstream**
        ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
        ///   * `OpenAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbconnection.openasync?view=netstandard-2.0#System_Data_Common_DbConnection_OpenAsync_System_Threading_CancellationToken_
        ///
        /// **Output Type**
        ///   * `Alt<unit>`
        member x.OpenAlt () =
            DbConnection.openConnection x

    type System.Data.Common.DbCommand with

        /// **Description**
        ///
        /// An `Alt` version of `ExecuteReaderAsync`
        ///
        /// **Upstream**
        ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
        ///   * `ExecuteReaderAsync` -  https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbcommand.executereaderasync?view=netstandard-2.0#System_Data_Common_DbCommand_ExecuteReaderAsync_System_Data_CommandBehavior_System_Threading_CancellationToken_
        ///
        /// **Parameters**
        ///   * `behavior` - parameter of type `System.Data.CommandBehavior`
        ///
        /// **Output Type**
        ///   * `Alt<DbDataReader>`
        member this.ExecuteReaderAlt (behavior : CommandBehavior) =
            DbCommand.executeReader behavior this

        /// **Description**
        ///
        /// An `Alt` version of `ExecuteNonQueryAsync`
        ///
        /// **Upstream**
        ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
        ///   * `ExecuteNonQueryAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbcommand.executenonqueryasync?view=netstandard-2.0#System_Data_Common_DbCommand_ExecuteNonQueryAsync_System_Threading_CancellationToken_
        ///
        /// **Output Type**
        ///   * `Alt<int>`
        member this.ExecuteNonQueryAlt () =
            DbCommand.executeNonQuery this

        /// **Description**
        ///
        /// An `Alt` version of `ExecuteScalarAsync`
        ///
        /// **Upstream**
        ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
        ///   * `ExecuteScalarAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbcommand.executescalarasync?view=netstandard-2.0#System_Data_Common_DbCommand_ExecuteScalarAsync_System_Threading_CancellationToken_
        ///
        /// **Output Type**
        ///   * `Alt<'a>`
        member this.ExecuteScalarAlt () =
            DbCommand.executeScalar this

    type System.Data.Common.DbDataReader with

        /// **Description**
        ///
        /// An `Alt` version of `ReadAsync`
        ///
        /// **Upstream**
        ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
        ///   * `ReadAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbdatareader.readasync?view=netstandard-2.0#System_Data_Common_DbDataReader_ReadAsync_System_Threading_CancellationToken_
        ///
        /// **Output Type**
        ///   * `Alt<bool>`
        member this.ReadAlt () =
            DbDataReader.read this

        /// **Description**
        ///
        /// An `Alt` version of `NextResultAsync`
        ///
        /// **Upstream**
        ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
        ///   * `NextResultAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbdatareader.nextresultasync?view=netstandard-2.0#System_Data_Common_DbDataReader_NextResultAsync_System_Threading_CancellationToken_
        ///
        /// **Output Type**
        ///   * `Alt<bool>`
        member this.NextResultAlt () =
            DbDataReader.nextResult this

        /// **Description**
        ///
        /// An `Alt` version of `GetFieldValueAsync`
        ///
        /// **Upstream**
        ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
        ///   * `GetFieldValueAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbdatareader.getfieldvalueasync?view=netstandard-2.0#System_Data_Common_DbDataReader_GetFieldValueAsync__1_System_Int32_System_Threading_CancellationToken_
        ///
        /// **WARNING**
        ///
        /// You only want to use this when using CommandBehavior.SequentialAccess. See: https://blogs.msdn.microsoft.com/adonet/2012/04/20/using-sqldatareaders-new-async-methods-in-net-4-5/
        ///
        /// **Parameters**
        ///   * `ordinal` - parameter of type `int` - The zero-based column to be retrieved.
        ///
        /// **Output Type**
        ///   * `Alt<'a>`
        member this.GetFieldValueAlt (ordinal : int) =
            DbDataReader.getFieldValue ordinal this

        /// **Description**
        ///
        /// An `Alt` version of `IsDBNullAsync`
        ///
        /// **Upstream**
        ///   * `Alt` - https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.Alt
        ///   * `IsDBNullAsync` - https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbdatareader.isdbnullasync?view=netstandard-2.0#System_Data_Common_DbDataReader_IsDBNullAsync_System_Int32_System_Threading_CancellationToken_
        ///
        /// **WARNING**
        ///
        /// You only want to use this when using CommandBehavior.SequentialAccess. See: https://blogs.msdn.microsoft.com/adonet/2012/04/20/using-sqldatareaders-new-async-methods-in-net-4-5/
        ///
        /// **Parameters**
        ///   * `ordinal` - parameter of type `int` - The zero-based column to be retrieved.
        ///   * `reader` - parameter of type `DbDataReader`
        ///
        /// **Output Type**
        ///   * `Alt<bool>`
        member this.IsDBNullAlt (ordinal : int) =
            DbDataReader.isDBNull ordinal this
