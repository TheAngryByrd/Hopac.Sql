namespace Tests

module Migrations =
    open SimpleMigrations

    [<Migration(20180809133213L, "Create people table")>]
    type ``Create people table`` () =
        inherit Migration()
            override __.Up () =
                base.Execute
                    """
                    CREATE TABLE people (
                        Id UUID PRIMARY KEY,
                        Name Text NOT NULL,
                        Age  integer NOT NULL,
                        DateOfBirth timestamptz NOT NULL,
                        Nickname text NULL
                    )
                    """
            override __.Down () =
                base.Execute """DROP TABLE people"""
