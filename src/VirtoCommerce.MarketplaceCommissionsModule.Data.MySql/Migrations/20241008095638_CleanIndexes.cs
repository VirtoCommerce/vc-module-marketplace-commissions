using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class CleanIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seller_CommissionFee_CommissionFeeId",
                table: "Seller");

            migrationBuilder.DropIndex(
                name: "IX_Seller_CommissionFeeId",
                table: "Seller");

            migrationBuilder.DropForeignKey(
                name: "FK_SellerCommission_Seller_SellerId",
                table: "SellerCommission");

            migrationBuilder.AddForeignKey(
                name: "FK_SellerCommission_Seller_SellerId",
                table: "SellerCommission",
                column: "SellerId",
                principalTable: "Seller",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Seller_CommissionFeeId",
                table: "Seller",
                column: "CommissionFeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Seller_CommissionFee_CommissionFeeId",
                table: "Seller",
                column: "CommissionFeeId",
                principalTable: "CommissionFee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
