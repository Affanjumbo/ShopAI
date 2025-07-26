using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopAI.Migrations
{
    /// <inheritdoc />
    public partial class Address : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SellerAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                        
                     SellerId   = table.Column<int>(nullable: false),

                    AddressType = table.Column<string>(maxLength: 100, nullable: false),

                    Street = table.Column<string>(maxLength: 200, nullable: false),

                    City = table.Column<string>(maxLength: 50, nullable: false),

                    State = table.Column<string>(maxLength: 50, nullable: false),

                    PostalCode = table.Column<string>(maxLength: 10, nullable: false),

                    Country = table.Column<string>(maxLength: 50, nullable: false),

                    IsDefault = table.Column<bool>(nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerAddresses_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SellerAddresses_SellerId",
                table: "SellerAddresses",
                column: "SellerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {


        }

    }
}
