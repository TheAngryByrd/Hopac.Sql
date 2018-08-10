namespace Tests
open System.Reflection
open Npgsql

module DatabaseTestHelpers =
    open System
    open Npgsql
    open SimpleMigrations
    open SimpleMigrations.DatabaseProvider
    let time name f x =
        let sw = System.Diagnostics.Stopwatch.StartNew()
        let r = f x
        sw.Stop()
        // printfn "%s" <|  sprintf "%s took %d" name sw.ElapsedMilliseconds
        r
    let execNonQuery connStr commandStr =
        use conn = new NpgsqlConnection(connStr)
        use cmd = new NpgsqlCommand(commandStr,conn)
        conn.Open()
        cmd.ExecuteNonQuery()

    let createDatabase connStr databaseName =
        databaseName
        |> sprintf "CREATE database \"%s\" ENCODING = 'UTF8'"
        |> execNonQuery connStr
        |> ignore

    let dropDatabase connStr databaseName =
        time "dropDatabase" (fun () ->
            //kill out all connections
            databaseName
            |> sprintf "select pg_terminate_backend(pid) from pg_stat_activity where datname='%s';"
            |> execNonQuery connStr
            |> ignore

            databaseName
            |> sprintf "DROP database \"%s\""
            |> execNonQuery connStr
            |> ignore
        ) ()

    let createConnString host user pass database =
        sprintf "Host=%s;Username=%s;Password=%s;Database=%s" host user pass database
        |> NpgsqlConnectionStringBuilder

    let duplicateConn (conn : NpgsqlConnectionStringBuilder) =
        conn |> string |> NpgsqlConnectionStringBuilder

    let duplicateAndChange f conn =
        let duplicate = duplicateConn conn
        duplicate |> f
        duplicate


    type DisposableDatabase (superConn : NpgsqlConnectionStringBuilder, databaseName : string) =
        static member Create(connStr) =
            let databaseName = System.Guid.NewGuid().ToString("n")
            createDatabase (connStr |> string) databaseName

            new DisposableDatabase(connStr,databaseName)
        member __.SuperConn = superConn
        member x.Conn =
            x.SuperConn
            |> duplicateAndChange (fun conn ->
                conn.Database <- x.DatabaseName
            )

        member x.DatabaseName = databaseName
        interface IDisposable with
            member x.Dispose() =
                dropDatabase (superConn |> string) databaseName

    let getEnv str =
        System.Environment.GetEnvironmentVariable str

    let getEnvOrDefault defaultVal key =
        let value = System.Environment.GetEnvironmentVariable key
        if value |> String.IsNullOrEmpty then
            defaultVal
        else
            value

    let host () =  "POSTGRES_HOST" |> getEnvOrDefault "localhost"
    let user () = "POSTGRES_USER" |> getEnvOrDefault "postgres"
    let pass () =  "POSTGRES_PASS" |> getEnvOrDefault  "postgres"
    let db () = "POSTGRES_DB" |> getEnvOrDefault "postgres"
    let superUserConnStr () =
        let connstr = createConnString (host ()) (user ()) (pass()) (db())
        connstr.Pooling <- true
        connstr.MaxAutoPrepare <- 100
        connstr




    let getNewDatabase () =
        time "getNewDatabase" (fun () ->
            superUserConnStr () |>  DisposableDatabase.Create) ()


    let doMigration (migrationsAssembly : Assembly) connStr =

        time "doMigration" (fun () ->
            use connection = new NpgsqlConnection(connStr)

            let databaseProvider = new PostgresqlDatabaseProvider(connection)
            let migrator = new SimpleMigrator(migrationsAssembly, databaseProvider)
            migrator.Load()
            migrator.MigrateToLatest()
        ) ()

    let getMigratedDatabase  =
        let migrationAssembly = Assembly.GetExecutingAssembly()
        let rec inner () =
            try
                let db = getNewDatabase ()
                db.Conn
                |> string
                |> doMigration migrationAssembly
                db
            with
            | :? PostgresException as e ->
                inner ()
            | ex -> reraise ()
        inner
