using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class bid_history : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Listings_AspNetUsers_HighestBidderId",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_HighestBidderId",
                table: "Listings");

            migrationBuilder.RenameColumn(
                name: "HighestBidderId",
                table: "Listings",
                newName: "HighestBidId");

            migrationBuilder.CreateTable(
                name: "AuctionBids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuctionId = table.Column<int>(type: "integer", nullable: false),
                    BidderId = table.Column<int>(type: "integer", nullable: false),
                    BidPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    BidDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionBids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuctionBids_AspNetUsers_BidderId",
                        column: x => x.BidderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuctionBids_Listings_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuctionBids_AuctionId",
                table: "AuctionBids",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionBids_BidderId",
                table: "AuctionBids",
                column: "BidderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuctionBids");

            migrationBuilder.RenameColumn(
                name: "HighestBidId",
                table: "Listings",
                newName: "HighestBidderId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_HighestBidderId",
                table: "Listings",
                column: "HighestBidderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_AspNetUsers_HighestBidderId",
                table: "Listings",
                column: "HighestBidderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
