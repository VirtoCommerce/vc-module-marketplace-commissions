using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerCommission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SellerCommission",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SellerId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommissionFeeId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerCommission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerCommission_CommissionFee_CommissionFeeId",
                        column: x => x.CommissionFeeId,
                        principalTable: "CommissionFee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SellerCommission_Seller_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Seller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SellerCommission_CommissionFeeId",
                table: "SellerCommission",
                column: "CommissionFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerCommission_SellerId",
                table: "SellerCommission",
                column: "SellerId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SellerCommission");
        }
    }
}
