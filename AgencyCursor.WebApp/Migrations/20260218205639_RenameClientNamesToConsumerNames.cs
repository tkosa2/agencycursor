using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgencyCursor.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class RenameClientNamesToConsumerNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClientNames",
                table: "Requests",
                newName: "ConsumerNames");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConsumerNames",
                table: "Requests",
                newName: "ClientNames");
        }
    }
}
