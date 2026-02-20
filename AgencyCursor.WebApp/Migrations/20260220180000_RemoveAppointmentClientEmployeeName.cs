using AgencyCursor.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgencyCursor.WebApp.Migrations
{
    [DbContext(typeof(AgencyDbContext))]
    [Migration("20260220180000_RemoveAppointmentClientEmployeeName")]
    public partial class RemoveAppointmentClientEmployeeName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("PRAGMA foreign_keys=OFF;", suppressTransaction: true);
            migrationBuilder.Sql(@"
CREATE TABLE ""Appointments_temp"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Appointments"" PRIMARY KEY AUTOINCREMENT,
    ""AdditionalNotes"" TEXT NULL,
    ""DurationMinutes"" INTEGER NULL,
    ""InterpreterId"" INTEGER NULL,
    ""Location"" TEXT NULL,
    ""RequestId"" INTEGER NOT NULL,
    ""ServiceDateTime"" TEXT NOT NULL,
    ""ServiceDetails"" TEXT NULL,
    ""Status"" TEXT NOT NULL,
    CONSTRAINT ""FK_Appointments_Interpreters_InterpreterId"" FOREIGN KEY (""InterpreterId"") REFERENCES ""Interpreters"" (""Id"") ON DELETE SET NULL,
    CONSTRAINT ""FK_Appointments_Requests_RequestId"" FOREIGN KEY (""RequestId"") REFERENCES ""Requests"" (""Id"") ON DELETE RESTRICT
);

INSERT INTO ""Appointments_temp"" (
    ""Id"",
    ""AdditionalNotes"",
    ""DurationMinutes"",
    ""InterpreterId"",
    ""Location"",
    ""RequestId"",
    ""ServiceDateTime"",
    ""ServiceDetails"",
    ""Status"")
SELECT
    ""Id"",
    ""AdditionalNotes"",
    ""DurationMinutes"",
    ""InterpreterId"",
    ""Location"",
    ""RequestId"",
    ""ServiceDateTime"",
    ""ServiceDetails"",
    ""Status""
FROM ""Appointments"";

DROP TABLE ""Appointments"";

ALTER TABLE ""Appointments_temp"" RENAME TO ""Appointments"";

CREATE INDEX ""IX_Appointments_InterpreterId"" ON ""Appointments"" (""InterpreterId"");
CREATE INDEX ""IX_Appointments_RequestId"" ON ""Appointments"" (""RequestId"");
");
            migrationBuilder.Sql("PRAGMA foreign_keys=ON;", suppressTransaction: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientEmployeeName",
                table: "Appointments",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }
    }
}
