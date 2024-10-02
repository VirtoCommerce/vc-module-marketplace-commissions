using System;
using Moq;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Models;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Unit
{
    public class SellerCommissionEntityTests
    {
        [Fact]
        public void ConvertSellerCommissionEntityToModel_Null_ThrowsArgumentNullException()
        {
            // Arrange
            SellerCommissionEntity sellerCommissionEntity = new SellerCommissionEntity();

            // Act
            Action actual = () => sellerCommissionEntity.ToModel(null);

            // Assertion
            Assert.Throws<ArgumentNullException>(actual);
        }

        [Fact]
        public void ConvertSellerCommissionEntityFromModel_Null_ThrowsArgumentNullException()
        {
            // Arrange
            SellerCommissionEntity sellerCommissionEntity = new SellerCommissionEntity();

            // Act
            Action actual = () => sellerCommissionEntity.FromModel(null, null);

            // Assertion
            Assert.Throws<ArgumentNullException>(actual);
        }

        [Theory]
        [MemberData(nameof(Input))]
        public void ConvertSellerCommissionEntityFromModelToModel_NotNull_ReturnsSameValue(SellerCommissionEntity originalSellerCommissionEntity)
        {
            // Arrange
            var pkMap = new Mock<PrimaryKeyResolvingMap>();

            // Act
            var convertedSellerCommission = originalSellerCommissionEntity.ToModel(new SellerCommission());
            var convertedSellerCommissionEntity = new SellerCommissionEntity().FromModel(convertedSellerCommission, pkMap.Object);

            // Assertion
            Assert.Equal(originalSellerCommissionEntity.Id, convertedSellerCommissionEntity.Id);
            Assert.Equal(originalSellerCommissionEntity.SellerId, convertedSellerCommissionEntity.SellerId);
            Assert.Equal(originalSellerCommissionEntity.CommissionFeeId, convertedSellerCommissionEntity.CommissionFeeId);
            Assert.Equal(originalSellerCommissionEntity.CreatedDate, convertedSellerCommissionEntity.CreatedDate);
            Assert.Equal(originalSellerCommissionEntity.ModifiedDate, convertedSellerCommissionEntity.ModifiedDate);
            Assert.Equal(originalSellerCommissionEntity.CreatedBy, convertedSellerCommissionEntity.CreatedBy);
            Assert.Equal(originalSellerCommissionEntity.ModifiedBy, convertedSellerCommissionEntity.ModifiedBy);
        }

        public static TheoryData<SellerCommissionEntity> Input()
        {
            return new TheoryData<SellerCommissionEntity>()
            {
                new SellerCommissionEntity
                {
                    Id = "TestId",
                    SellerId = "SellerTestId",
                    CommissionFeeId = "CommissionFeeTestId",
                    CreatedDate = new DateTime(2024, 09, 19),
                    CreatedBy = "Initial data seed"
                }
            };
        }
    }
}
