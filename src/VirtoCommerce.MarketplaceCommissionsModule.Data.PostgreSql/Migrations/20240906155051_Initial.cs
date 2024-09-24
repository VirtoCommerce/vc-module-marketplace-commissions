using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createCommissionFeeTableScript = @"
                CREATE TABLE IF NOT EXISTS CommissionFee (
                    Id character varying(128) NOT NULL PRIMARY KEY,
                    Name character varying(256) NULL,
                    Description text NULL,
                    Type character varying(128) NOT NULL, 
                    CalculationType character varying(128) NOT NULL,
                    Fee money NOT NULL,
                    Priority integer NOT NULL,
                    IsActive boolean NOT NULL,
                    IsDefault boolean NOT NULL,
                    PredicateVisualTreeSerialized text NULL,
                    CreatedDate timestamp with time zone NOT NULL,
                    ModifiedDate timestamp with time zone NULL,
                    CreatedBy character varying(64) NULL,
                    ModifiedBy character varying(64) NULL)
            ";

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
