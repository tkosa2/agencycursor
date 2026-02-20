using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgencyCursor.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddInternationalOtherAndOtherInterpreter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InternationalOther",
                table: "Requests",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherInterpreter",
                table: "Requests",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InternationalOther",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "OtherInterpreter",
                table: "Requests");
        }
    }
}
