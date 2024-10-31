using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gauniv.WebServer.Migrations
{
    /// <inheritdoc />
    public partial class PayloadColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Payload",
                table: "Games",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Payload",
                table: "Games");
        }
    }
}
