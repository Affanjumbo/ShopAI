using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopAI.Migrations
{
    /// <inheritdoc />
    public partial class AddressChanging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultBilling",
                table: "Addresses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultShipping",
                table: "Addresses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefaultBilling",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "IsDefaultShipping",
                table: "Addresses");
        }
    }
}
