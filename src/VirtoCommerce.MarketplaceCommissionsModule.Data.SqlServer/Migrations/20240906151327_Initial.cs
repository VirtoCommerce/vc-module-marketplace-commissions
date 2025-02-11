using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createCommissionFeeTableScript = @"
                IF OBJECT_ID(N'dbo.CommissionFee', N'U') IS NULL
                    CREATE TABLE CommissionFee (
                              Id nvarchar(128) NOT NULL PRIMARY KEY,
                              Name nvarchar(256) NULL,
                              Description nvarchar(max) NULL,
                              Type nvarchar(128) NOT NULL, 
                              CalculationType nvarchar(128) NOT NULL,
                              Fee money NOT NULL,
                              Priority int NOT NULL,
                              IsActive bit NOT NULL,
                              IsDefault bit NOT NULL,
                              PredicateVisualTreeSerialized nvarchar(max) NULL,
                              CreatedDate datetime2 NOT NULL,
                              ModifiedDate datetime2 NULL,
                              CreatedBy nvarchar(64) NULL,
                              ModifiedBy nvarchar(64) NULL
                    );";

            migrationBuilder.Sql(createCommissionFeeTableScript);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommissionFee");
        }
    }
}
