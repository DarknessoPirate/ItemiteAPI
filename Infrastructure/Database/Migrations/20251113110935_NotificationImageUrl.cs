using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class NotificationImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResourceId",
                table: "Notifications");

            migrationBuilder.AddColumn<string>(
                name: "NotificationImageUrl",
                table: "Notifications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationImageUrl",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "ResourceId",
                table: "Notifications",
                type: "integer",
                nullable: true);
        }
    }
}
