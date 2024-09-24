using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class CommissionDataTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var commissionDataTransferScript = @"
                DO $$
                DECLARE CurrentItemSellerId VARCHAR(100);
                DECLARE CurrentItemCommissionFeeId VARCHAR(100);
                DECLARE SNCursor CURSOR FOR 	
                    SELECT ""Id"", ""CommissionFeeId""
                        FROM public.""Seller""
                        WHERE ""CommissionFeeId"" IS NOT NULL;

                BEGIN
                OPEN SNCursor;

                LOOP    
                    FETCH NEXT FROM SNCursor INTO CurrentItemSellerId, CurrentItemCommissionFeeId;
                    EXIT WHEN NOT FOUND;

                    INSERT INTO public.""SellerCommission""
		                (""Id"", ""SellerId"", ""CommissionFeeId"", ""CreatedDate"", ""CreatedBy"")
                    VALUES (gen_random_uuid(), CurrentItemSellerId, CurrentItemCommissionFeeId, NOW(), 'Script');

                END LOOP;

                END $$;
                ";

            migrationBuilder.Sql(commissionDataTransferScript);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
