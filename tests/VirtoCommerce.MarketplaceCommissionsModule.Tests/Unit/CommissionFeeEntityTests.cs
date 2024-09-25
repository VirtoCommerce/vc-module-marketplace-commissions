using System;
using System.Collections.Generic;
using Moq;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Models;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Unit
{
    public class CommissionFeeEntityTests
    {
        [Fact]
        public void ConvertCommissionFeeEntityToModel_Null_ThrowsArgumentNullException()
        {
            // Arrange
            CommissionFeeEntity commissionFeeEntity = new CommissionFeeEntity();

            // Act
            Action actual = () => commissionFeeEntity.ToModel(null);

            // Assertion
            Assert.Throws<ArgumentNullException>(actual);
        }

        [Fact]
        public void ConvertCommissionFeeEntityFromModel_Null_ThrowsArgumentNullException()
        {
            // Arrange
            CommissionFeeEntity commissionFeeEntity = new CommissionFeeEntity();

            // Act
            Action actual = () => commissionFeeEntity.FromModel(null, null);

            // Assertion
            Assert.Throws<ArgumentNullException>(actual);
        }

        [Fact]
        public void ConvertCommissionFeeEntityFromModelToModel_NotNull_ReturnsSameValue()
        {
            // Arrange
            var originalCommissionFeeEntities = TestHepler.LoadFromJsonFile<CommissionFeeEntity[]>(@"commissionFees.json");
            var convertedCommissionFeeEntities = new List<CommissionFeeEntity>();
            var pkMap = new Mock<PrimaryKeyResolvingMap>();
            int index = 0;

            // Act
            foreach (var originalCommissionFeeEntity in originalCommissionFeeEntities)
            {
                var convertedCommissionFee = originalCommissionFeeEntity.ToModel(new CommissionFee());
                var convertedCommissionFeeEntity = new CommissionFeeEntity().FromModel(convertedCommissionFee, pkMap.Object);
                convertedCommissionFeeEntities.Add(convertedCommissionFeeEntity);
            }

            // Assertion
            foreach (var originalCommissionFeeEntity in originalCommissionFeeEntities)
            {
                var convertedCommissionFeeEntity = convertedCommissionFeeEntities[index++];

                Assert.Equal(originalCommissionFeeEntity.Id, convertedCommissionFeeEntity.Id);
                Assert.Equal(originalCommissionFeeEntity.Name, convertedCommissionFeeEntity.Name);
                Assert.Equal(originalCommissionFeeEntity.Description, convertedCommissionFeeEntity.Description);
                Assert.Equal(originalCommissionFeeEntity.Type, convertedCommissionFeeEntity.Type);
                Assert.Equal(originalCommissionFeeEntity.CalculationType, convertedCommissionFeeEntity.CalculationType);
                Assert.Equal(originalCommissionFeeEntity.Fee, convertedCommissionFeeEntity.Fee);
                Assert.Equal(originalCommissionFeeEntity.Priority, convertedCommissionFeeEntity.Priority);
                Assert.Equal(originalCommissionFeeEntity.IsDefault, convertedCommissionFeeEntity.IsDefault);
                Assert.Equal(originalCommissionFeeEntity.PredicateVisualTreeSerialized, convertedCommissionFeeEntity.PredicateVisualTreeSerialized);
                Assert.Equal(originalCommissionFeeEntity.CreatedDate, convertedCommissionFeeEntity.CreatedDate);
                Assert.Equal(originalCommissionFeeEntity.ModifiedDate, convertedCommissionFeeEntity.ModifiedDate);
                Assert.Equal(originalCommissionFeeEntity.CreatedBy, convertedCommissionFeeEntity.CreatedBy);
                Assert.Equal(originalCommissionFeeEntity.ModifiedBy, convertedCommissionFeeEntity.ModifiedBy);
            }
        }
    }
}
