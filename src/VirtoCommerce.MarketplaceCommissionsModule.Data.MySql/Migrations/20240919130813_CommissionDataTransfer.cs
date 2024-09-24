using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class CommissionDataTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var commissionDataTransferScript = @"
                CREATE PROCEDURE datatransfer()
                BEGIN
                    DECLARE done INT DEFAULT FALSE;
                    DECLARE CurrentItemSellerId VARCHAR(100);
                    DECLARE CurrentItemCommissionFeeId VARCHAR(100);
                    DECLARE SNCursor CURSOR FOR 	
                        SELECT `Id`, `CommissionFeeId`
                            FROM `Seller`
                            WHERE `CommissionFeeId` IS NOT NULL;
                    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;

                    OPEN SNCursor;

                    seller_loop: LOOP
                        FETCH SNCursor INTO CurrentItemSellerId, CurrentItemCommissionFeeId;
                        IF done THEN LEAVE seller_loop;
                        END IF;

                        INSERT INTO `SellerCommission`
                            (`Id`, `SellerId`, `CommissionFeeId`, `CreatedDate`, `CreatedBy`)
                        SELECT
                            UUID(), CurrentItemSellerId, CurrentItemCommissionFeeId, NOW(), 'Script'
                            FROM dual;

                    END LOOP;

                    CLOSE SNCursor;
                END;

                CALL datatransfer();

                DROP PROCEDURE datatransfer;
            ";
            migrationBuilder.Sql(commissionDataTransferScript);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
