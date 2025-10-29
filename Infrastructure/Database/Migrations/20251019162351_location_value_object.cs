using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class location_value_object : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Location_City",
                table: "Listings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location_Country",
                table: "Listings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Location_Latitude",
                table: "Listings",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Location_Longitude",
                table: "Listings",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Location_PostalCode",
                table: "Listings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location_City",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location_Country",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Location_Latitude",
                table: "AspNetUsers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Location_Longitude",
                table: "AspNetUsers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Location_PostalCode",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location_City",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Location_Country",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Location_Latitude",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Location_Longitude",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Location_PostalCode",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Location_City",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Location_Country",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Location_Latitude",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Location_Longitude",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Location_PostalCode",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Listings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }
    }
}
