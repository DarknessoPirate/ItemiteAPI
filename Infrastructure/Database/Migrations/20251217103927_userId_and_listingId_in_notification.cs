using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class userId_and_listingId_in_notification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationImageUrl",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "ResourceId",
                table: "Notifications",
                newName: "UserId");

            migrationBuilder.AddColumn<int>(
                name: "ListingId",
                table: "Notifications",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListingId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Notifications",
                newName: "ResourceId");

            migrationBuilder.AddColumn<string>(
                name: "NotificationImageUrl",
                table: "Notifications",
                type: "text",
                nullable: true);
        }
    }
}
