using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class CommissionDataTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var commissionDataTransferScript = @"
                DECLARE @CurrentItemSellerId nvarchar(100), @CurrentItemCommissionFeeId nvarchar(100)

                DECLARE [SNCursor] CURSOR LOCAL FOR
	                SELECT [Id], [CommissionFeeId]
	                FROM [Seller]
	                WHERE [CommissionFeeId] IS NOT NULL

                OPEN [SNCursor]
                FETCH NEXT FROM [SNCursor] INTO @CurrentItemSellerId, @CurrentItemCommissionFeeId
                WHILE @@FETCH_STATUS=0
                BEGIN
	                INSERT INTO [SellerCommission]
		                ([Id], [SellerId], [CommissionFeeId], [CreatedDate], [CreatedBy])
                    VALUES (CONVERT(varchar(128), NEWID()), @CurrentItemSellerId, @CurrentItemCommissionFeeId, GETDATE(), 'Script')
                    FETCH NEXT FROM [SNCursor] INTO @CurrentItemSellerId, @CurrentItemCommissionFeeId
                END
                CLOSE [SNCursor]
                DEALLOCATE [SNCursor]
                ";

            migrationBuilder.Sql(commissionDataTransferScript);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
