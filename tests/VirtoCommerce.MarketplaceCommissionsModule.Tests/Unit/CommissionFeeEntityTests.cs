using System;
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

        [Theory]
        [MemberData(nameof(Input))]
        public void ConvertCommissionFeeEntityFromModelToModel_NotNull_ReturnsSameValue(CommissionFeeEntity originalCommissionFeeEntity)
        {
            // Arrange
            var pkMap = new Mock<PrimaryKeyResolvingMap>();

            // Act
            var convertedCommissionFee = originalCommissionFeeEntity.ToModel(new CommissionFee());
            var convertedCommissionFeeEntity = new CommissionFeeEntity().FromModel(convertedCommissionFee, pkMap.Object);

            // Assertion
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

        public static TheoryData<CommissionFeeEntity> Input()
        {
            return new TheoryData<CommissionFeeEntity>()
            {
                new CommissionFeeEntity
                {
                    Id = "CommissionFeeTestId",
                    Name = "My test commission",
                    Description = "My test commission description",
                    Type = CommissionFeeType.Static,
                    CalculationType = FeeCalculationType.Fixed,
                    Fee = 1,
                    Priority = 0,
                    IsActive = true,
                    IsDefault = true,
                    PredicateVisualTreeSerialized = null,
                    CreatedDate = new DateTime(2024, 09, 19),
                    CreatedBy = "Initial data seed"
                }
            };
        }
    }
}
