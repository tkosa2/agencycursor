using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgencyCursor.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddInterpreterEmailLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterpreterEmailLogs",
                columns: table => new
                {
                    InterpreterEmailLogId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestId = table.Column<int>(type: "INTEGER", nullable: false),
                    InterpreterId = table.Column<int>(type: "INTEGER", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterpreterEmailLogs", x => x.InterpreterEmailLogId);
                    table.ForeignKey(
                        name: "FK_InterpreterEmailLogs_Interpreters_InterpreterId",
                        column: x => x.InterpreterId,
                        principalTable: "Interpreters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterpreterEmailLogs_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterpreterResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestId = table.Column<int>(type: "INTEGER", nullable: false),
                    InterpreterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResponseToken = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterpreterResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterpreterResponses_Interpreters_InterpreterId",
                        column: x => x.InterpreterId,
                        principalTable: "Interpreters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterpreterResponses_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterpreterEmailLogs_InterpreterId",
                table: "InterpreterEmailLogs",
                column: "InterpreterId");

            migrationBuilder.CreateIndex(
                name: "IX_InterpreterEmailLogs_RequestId",
                table: "InterpreterEmailLogs",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_InterpreterResponses_InterpreterId",
                table: "InterpreterResponses",
                column: "InterpreterId");

            migrationBuilder.CreateIndex(
                name: "IX_InterpreterResponses_RequestId",
                table: "InterpreterResponses",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterpreterEmailLogs");

            migrationBuilder.DropTable(
                name: "InterpreterResponses");
        }
    }
}
