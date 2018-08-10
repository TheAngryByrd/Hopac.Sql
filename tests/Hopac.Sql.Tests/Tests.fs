namespace Tests

open Expecto
open ExpectoHelpers
open Hopac
open Hopac.Infixes
open Hopac.Sql
open System

open Npgsql
open Expecto.Logging
open System.Data
open System.Data.Common
open Npgsql
open DatabaseTestHelpers

module Tests =
    // do NpgsqlConnection.GlobalTypeMapper.UseNodaTime() |> ignore
    let inline withMigratedDatabase (f : _ -> Job<unit>)  = job {
        use! database =
            Job.onThreadPool (DatabaseTestHelpers.getMigratedDatabase)
        return! f (database)
    }

    type Person = {
        Id : Guid
        Name : string
        Age : int
        DateOfBirth : DateTime
        Nickname : string option
    }

    [<Tests>]
    let ``Query Tests`` =
        testList "Query Tests" [
            yield! testFixtureAsync withMigratedDatabase [
                yield testCaseJob' "Can handle None values" <| fun db -> job {
                    let person = {
                        Id = Guid.NewGuid()
                        Name = "Jim Kirk"
                        Age = 34
                        DateOfBirth = DateTime.MinValue
                        Nickname = None
                    }
                    use conn = new NpgsqlConnection(db.Conn.ToString())
                    let commandText = """
                        INSERT INTO
                            people (Id,Name,Age,DateOfBirth,Nickname)
                            VALUES (@id,@name,@age,@dob,@nickname)
                        """
                    let param = [
                        ("id", person.Id |> box)
                        ("name", person.Name |> box)
                        ("age", person.Age |> box)
                        ("dob", person.DateOfBirth |> box)
                        ("nickname", person.Nickname |> box)
                    ]
                    use cmd = new NpgsqlCommand(commandText,conn)
                    param
                    |> Seq.map(Sql.normalizeParameters)
                    |> Seq.iter(fun (key,value) -> cmd.Parameters.AddWithValue(key,value) |> ignore)
                    do! DbConnection.openConnection conn
                    let! resultCount = DbCommand.executeNonQuery cmd
                    Expect.equal resultCount 1 "Should have added 1 record to people"



                    let query = """
                        SELECT Id, Name, Age, DateOfBirth, Nickname
                        FROM people
                    """
                    let! actualPerson =
                        Sql.queryList<Person> conn query [||]
                        |> Job.map(Seq.head)
                    Expect.equal actualPerson person "Should be same person"

                }
                yield testCaseJob' "Can handle Some(value) in parameters" <| fun db -> job {
                    let person = {
                        Id = Guid.NewGuid()
                        Name = "Jim Kirk"
                        Age = 34
                        DateOfBirth = DateTime.MinValue
                        Nickname = Some("Jerk")
                    }
                    use conn = new NpgsqlConnection(db.Conn.ToString())
                    let commandText = """
                        INSERT INTO
                            people (Id,Name,Age,DateOfBirth,Nickname)
                            VALUES (@id,@name,@age,@dob,@nickname)
                        """
                    let param = [
                        ("id", person.Id |> box)
                        ("name", person.Name |> box)
                        ("age", Some(person.Age) |> box)
                        ("dob", person.DateOfBirth |> box)
                        ("nickname", person.Nickname |> box)
                    ]
                    use cmd = new NpgsqlCommand(commandText,conn)
                    param
                    |> Seq.map(Sql.normalizeParameters)
                    |> Seq.iter(fun (key,value) -> cmd.Parameters.AddWithValue(key,value) |> ignore)
                    do! DbConnection.openConnection conn
                    let! resultCount = DbCommand.executeNonQuery cmd
                    Expect.equal resultCount 1 "Should have added 1 record to people"

                    let query = """
                        SELECT Id, Name, Age, DateOfBirth, Nickname
                        FROM people
                    """
                    let! actualPerson =
                        Sql.queryList<Person> conn query [||]
                        |> Job.map(Seq.head)
                    Expect.equal actualPerson person "Should be same person"
                }
                yield testCaseJob' "Can handle null in parameters" <| fun db -> job {
                    let person = {
                        Id = Guid.NewGuid()
                        Name = "Jim Kirk"
                        Age = 34
                        DateOfBirth = DateTime.MinValue
                        Nickname = None
                    }
                    use conn = new NpgsqlConnection(db.Conn.ToString())
                    let commandText = """
                        INSERT INTO
                            people (Id,Name,Age,DateOfBirth,Nickname)
                            VALUES (@id,@name,@age,@dob,@nickname)
                        """
                    let param = [
                        ("id", person.Id |> box)
                        ("name", person.Name |> box)
                        ("age", person.Age |> box)
                        ("dob", person.DateOfBirth |> box)
                        ("nickname", null)
                    ]
                    use cmd = new NpgsqlCommand(commandText,conn)
                    param
                    |> Seq.map(Sql.normalizeParameters)
                    |> Seq.iter(fun (key,value) -> cmd.Parameters.AddWithValue(key,value) |> ignore)
                    do! DbConnection.openConnection conn
                    let! resultCount = DbCommand.executeNonQuery cmd
                    Expect.equal resultCount 1 "Should have added 1 record to people"

                    let query = """
                        SELECT Id, Name, Age, DateOfBirth, Nickname
                        FROM people
                    """
                    let! actualPerson =
                        Sql.queryList<Person> conn query [||]
                        |> Job.map(Seq.head)
                    Expect.equal actualPerson person "Should be same person"
                }
                yield testCaseJob' "Can Handle out of order properties" <| fun db -> job {
                    let person = {
                        Id = Guid.NewGuid()
                        Name = "Jim Kirk"
                        Age = 34
                        DateOfBirth = DateTime.MinValue
                        Nickname = None
                    }
                    use conn = new NpgsqlConnection(db.Conn.ToString())
                    let commandText = """
                        INSERT INTO
                            people (Id,Name,Age,DateOfBirth,Nickname)
                            VALUES (@id,@name,@age,@dob,@nickname)
                        """
                    let param = [
                        ("id", person.Id |> box)
                        ("name", person.Name |> box)
                        ("age", person.Age |> box)
                        ("dob", person.DateOfBirth |> box)
                        ("nickname", person.Nickname |> box)
                    ]
                    use cmd = new NpgsqlCommand(commandText,conn)
                    param
                    |> Seq.map(Sql.normalizeParameters)
                    |> Seq.iter(fun (key,value) -> cmd.Parameters.AddWithValue(key,value) |> ignore)
                    do! DbConnection.openConnection conn
                    let! resultCount = DbCommand.executeNonQuery cmd
                    Expect.equal resultCount 1 "Should have added 1 record to people"

                    let query = """
                        SELECT Id, Name, Age, DateOfBirth, Nickname
                        FROM people
                    """
                    let! actualPerson =
                        Sql.queryList<Person> conn query [||]
                        |> Job.map(Seq.head)
                    Expect.equal actualPerson person "Should be same person"
                }
                yield testCaseJob' "Can Generic Insert Single" <| fun db -> job {
                    let person = {
                        Id = Guid.NewGuid()
                        Name = "Jim Kirk"
                        Age = 34
                        DateOfBirth = DateTime.MinValue
                        Nickname = None
                    }
                    use conn = new NpgsqlConnection(db.Conn.ToString())
                    let! resultCount = conn |> Sql.simpleInsert "people" [|person|]

                    Expect.equal resultCount 1 "Should have added 1 record to people"

                    let query = """
                        SELECT Id, Name, Age, DateOfBirth, Nickname
                        FROM people
                    """
                    let! actualPerson =
                        Sql.queryList<Person> conn query [||]
                        |> Job.map(Seq.head)
                    Expect.equal actualPerson person "Should be same person"
                }
                yield testCaseJob' "Can query scalar" <| fun db -> job {
                    let person = {
                        Id = Guid.NewGuid()
                        Name = "Jim Kirk"
                        Age = 34
                        DateOfBirth = DateTime.MinValue
                        Nickname = None
                    }
                    use conn = new NpgsqlConnection(db.Conn.ToString())
                    let! resultCount = conn |> Sql.simpleInsert "people" [|person|]

                    Expect.equal resultCount 1 "Should have added 1 record to people"

                    let query = """
                        SELECT COUNT(*)
                        FROM people
                    """
                    let! countPeople =
                        Sql.queryScalar<int64> conn query [||]
                    Expect.equal 1L countPeople "Should have one record in table"
                }
            ]
        ]
