using System;
using System.Collections.Generic;
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

        [Fact]
        public void ConvertSellerCommissionEntityFromModelToModel_NotNull_ReturnsSameValue()
        {
            // Arrange
            var originalSellerCommissionEntities = TestHepler.LoadFromJsonFile<SellerCommissionEntity[]>(@"sellerCommissions.json");
            var convertedSellerCommissionEntities = new List<SellerCommissionEntity>();
            var pkMap = new Mock<PrimaryKeyResolvingMap>();
            int index = 0;

            // Act
            foreach (var originalSellerCommissionEntity in originalSellerCommissionEntities)
            {
                var convertedSellerCommission = originalSellerCommissionEntity.ToModel(new SellerCommission());
                var convertedSellerCommissionEntity = new SellerCommissionEntity().FromModel(convertedSellerCommission, pkMap.Object);
                convertedSellerCommissionEntities.Add(convertedSellerCommissionEntity);
            }

            // Assertion
            foreach (var originalSellerCommissionEntity in originalSellerCommissionEntities)
            {
                var convertedSellerCommissionEntity = convertedSellerCommissionEntities[index++];

                Assert.Equal(originalSellerCommissionEntity.Id, convertedSellerCommissionEntity.Id);
                Assert.Equal(originalSellerCommissionEntity.SellerId, convertedSellerCommissionEntity.SellerId);
                Assert.Equal(originalSellerCommissionEntity.CommissionFeeId, convertedSellerCommissionEntity.CommissionFeeId);
                Assert.Equal(originalSellerCommissionEntity.CreatedDate, convertedSellerCommissionEntity.CreatedDate);
                Assert.Equal(originalSellerCommissionEntity.ModifiedDate, convertedSellerCommissionEntity.ModifiedDate);
                Assert.Equal(originalSellerCommissionEntity.CreatedBy, convertedSellerCommissionEntity.CreatedBy);
                Assert.Equal(originalSellerCommissionEntity.ModifiedBy, convertedSellerCommissionEntity.ModifiedBy);
            }
        }
    }
}
