using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopAI.Migrations
{
    /// <inheritdoc />
    public partial class Program : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SellerId",
                table: "BankDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankDetails_SellerId",
                table: "BankDetails",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankDetails_Sellers_SellerId",
                table: "BankDetails",
                column: "SellerId",
                principalTable: "Sellers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankDetails_Sellers_SellerId",
                table: "BankDetails");

            migrationBuilder.DropIndex(
                name: "IX_BankDetails_SellerId",
                table: "BankDetails");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "BankDetails");
        }
    }
}
