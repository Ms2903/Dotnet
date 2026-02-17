using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsBookingPlatform.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRatingEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Games_GameId",
                table: "Ratings");

            migrationBuilder.AlterColumn<Guid>(
                name: "GameId",
                table: "Ratings",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Games_GameId",
                table: "Ratings",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Games_GameId",
                table: "Ratings");

            migrationBuilder.AlterColumn<Guid>(
                name: "GameId",
                table: "Ratings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Games_GameId",
                table: "Ratings",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "GameId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
