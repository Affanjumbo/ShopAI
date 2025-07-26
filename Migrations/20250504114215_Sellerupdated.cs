using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopAI.Migrations
{
    /// <inheritdoc />
    public partial class Sellerupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Sellers");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "Sellers",
                newName: "DateOfBirth");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Sellers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Sellers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Sellers");

            migrationBuilder.RenameColumn(
                name: "DateOfBirth",
                table: "Sellers",
                newName: "State");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Sellers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
