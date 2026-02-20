using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgencyCursor.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentInterpreterTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Interpreters_InterpreterId",
                table: "Appointments");

            migrationBuilder.AlterColumn<int>(
                name: "InterpreterId",
                table: "Invoices",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "InterpreterId",
                table: "Appointments",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateTable(
                name: "AppointmentInterpreters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppointmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    InterpreterId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentInterpreters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentInterpreters_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppointmentInterpreters_Interpreters_InterpreterId",
                        column: x => x.InterpreterId,
                        principalTable: "Interpreters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInterpreters_AppointmentId",
                table: "AppointmentInterpreters",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInterpreters_InterpreterId",
                table: "AppointmentInterpreters",
                column: "InterpreterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Interpreters_InterpreterId",
                table: "Appointments",
                column: "InterpreterId",
                principalTable: "Interpreters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Interpreters_InterpreterId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "AppointmentInterpreters");

            migrationBuilder.AlterColumn<int>(
                name: "InterpreterId",
                table: "Invoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InterpreterId",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Interpreters_InterpreterId",
                table: "Appointments",
                column: "InterpreterId",
                principalTable: "Interpreters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
