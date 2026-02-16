using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgencyCursor.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Interpreters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Languages = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Availability = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    AddressLine1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    AddressLine2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ZipCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Certification = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsRegisteredWithAgency = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interpreters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Requestors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Address2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    ZipCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requestors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZipCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CountryCode = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PlaceName = table.Column<string>(type: "TEXT", maxLength: 180, nullable: true),
                    AdminName1 = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AdminCode1 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    AdminName2 = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AdminCode2 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    AdminName3 = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AdminCode3 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    Accuracy = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZipCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestorId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NumberOfIndividuals = table.Column<int>(type: "INTEGER", nullable: false),
                    IndividualType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TypeOfService = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TypeOfServiceOther = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    MeetingLink = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Address2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    ZipCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    GenderPreference = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PreferredInterpreterId = table.Column<int>(type: "INTEGER", nullable: true),
                    PreferredInterpreterName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Specializations = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ServiceDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    AdditionalNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Interpreters_PreferredInterpreterId",
                        column: x => x.PreferredInterpreterId,
                        principalTable: "Interpreters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Requests_Requestors_RequestorId",
                        column: x => x.RequestorId,
                        principalTable: "Requestors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestId = table.Column<int>(type: "INTEGER", nullable: false),
                    InterpreterId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ServiceDetails = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    ClientEmployeeName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    AdditionalNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Interpreters_InterpreterId",
                        column: x => x.InterpreterId,
                        principalTable: "Interpreters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestorId = table.Column<int>(type: "INTEGER", nullable: false),
                    AppointmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    InterpreterId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    HoursWorked = table.Column<decimal>(type: "TEXT", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Interpreters_InterpreterId",
                        column: x => x.InterpreterId,
                        principalTable: "Interpreters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Requestors_RequestorId",
                        column: x => x.RequestorId,
                        principalTable: "Requestors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_InterpreterId",
                table: "Appointments",
                column: "InterpreterId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_RequestId",
                table: "Appointments",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AppointmentId",
                table: "Invoices",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InterpreterId",
                table: "Invoices",
                column: "InterpreterId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_RequestorId",
                table: "Invoices",
                column: "RequestorId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_PreferredInterpreterId",
                table: "Requests",
                column: "PreferredInterpreterId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequestorId",
                table: "Requests",
                column: "RequestorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "ZipCodes");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "Interpreters");

            migrationBuilder.DropTable(
                name: "Requestors");
        }
    }
}
