using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class modifiedbidentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Listings_Payments_PaymentId",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ListingId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Listings_PaymentId",
                table: "Listings");

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentClientSecret",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentIntentStatus",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentId",
                table: "AuctionBids",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ListingId",
                table: "Payments",
                column: "ListingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuctionBids_PaymentId",
                table: "AuctionBids",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionBids_Payments_PaymentId",
                table: "AuctionBids",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionBids_Payments_PaymentId",
                table: "AuctionBids");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ListingId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_AuctionBids_PaymentId",
                table: "AuctionBids");

            migrationBuilder.DropColumn(
                name: "PaymentIntentClientSecret",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentIntentStatus",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "AuctionBids");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ListingId",
                table: "Payments",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_PaymentId",
                table: "Listings",
                column: "PaymentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_Payments_PaymentId",
                table: "Listings",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
