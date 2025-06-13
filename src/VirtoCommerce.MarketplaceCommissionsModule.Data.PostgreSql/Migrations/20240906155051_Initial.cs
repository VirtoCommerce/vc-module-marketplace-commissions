using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS ""CommissionFee"" (
                  ""Id"" varchar(128) NOT NULL DEFAULT gen_random_uuid(),
                  ""Name"" varchar(254) NULL,
                  ""Description"" text NULL,
                  ""Type"" varchar(128) NOT NULL,
                  ""CalculationType"" varchar(128) NOT NULL,
                  ""Fee"" numeric(18,4) NOT NULL,
                  ""Priority"" integer NOT NULL,
                  ""IsActive"" boolean NOT NULL,
                  ""IsDefault"" boolean NOT NULL,
                  ""PredicateVisualTreeSerialized"" text NULL,
                  ""CreatedDate"" timestamp with time zone NOT NULL,
                  ""ModifiedDate"" timestamp with time zone NULL,
                  ""CreatedBy"" varchar(64) NULL,
                  ""ModifiedBy"" varchar(64) NULL,
                  PRIMARY KEY(""Id"")
                );";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""CommissionFee"";");
        }
    }
}
