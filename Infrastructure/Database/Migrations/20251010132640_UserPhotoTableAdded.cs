using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UserPhotoTableAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BackgroundPhotoId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfilePhotoId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserPhoto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Url = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPhoto", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BackgroundPhotoId",
                table: "AspNetUsers",
                column: "BackgroundPhotoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ProfilePhotoId",
                table: "AspNetUsers",
                column: "ProfilePhotoId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserPhoto_BackgroundPhotoId",
                table: "AspNetUsers",
                column: "BackgroundPhotoId",
                principalTable: "UserPhoto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserPhoto_ProfilePhotoId",
                table: "AspNetUsers",
                column: "ProfilePhotoId",
                principalTable: "UserPhoto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserPhoto_BackgroundPhotoId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserPhoto_ProfilePhotoId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "UserPhoto");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BackgroundPhotoId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ProfilePhotoId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BackgroundPhotoId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoId",
                table: "AspNetUsers");
        }
    }
}
