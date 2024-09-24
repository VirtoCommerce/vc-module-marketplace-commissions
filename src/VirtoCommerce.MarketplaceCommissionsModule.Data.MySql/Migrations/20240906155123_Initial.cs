using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            var createCommissionFeeTableScript = @"
                    CREATE TABLE IF NOT EXISTS `CommissionFee` (
                      `Id` varchar(128) NOT NULL default (uuid()) PRIMARY KEY,
                      `Name` varchar(254) NULL,
                      `Description` longtext NULL,
                      `Type` varchar(128) NOT NULL,
                      `CalculationType` varchar(128) NOT NULL,
                      `Fee` decimal(18, 4) NOT NULL,
                      `Priority` int NOT NULL,
                      `IsActive` boolean NOT NULL,
                      `IsDefault` boolean NOT NULL,
                      `PredicateVisualTreeSerialized` longtext NULL,
                      `CreatedDate` datetime(6) NOT NULL,
                      `ModifiedDate` datetime(6) NULL,
                      `CreatedBy` varchar(64) NULL,
                      `ModifiedBy` varchar(64) NULL
                    )
                ";

            migrationBuilder.Sql(createCommissionFeeTableScript)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommissionFee");
        }
    }
}
