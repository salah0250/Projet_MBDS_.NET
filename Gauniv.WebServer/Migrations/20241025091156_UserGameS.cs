using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Gauniv.WebServer.Migrations
{
    /// <inheritdoc />
    public partial class UserGameS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGames",
                table: "UserGames");

            migrationBuilder.DropIndex(
                name: "IX_UserGames_UserId",
                table: "UserGames");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "UserGames",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGames",
                table: "UserGames",
                columns: new[] { "UserId", "GameId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGames",
                table: "UserGames");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "UserGames",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGames",
                table: "UserGames",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserGames_UserId",
                table: "UserGames",
                column: "UserId");
        }
    }
}
