using System;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using Xunit;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Unit
{
    public class CommissionFeeModelTests
    {
        [Fact]
        public void CommissionFeeCreateNewFromDetails_Null_ThrowsArgumentNullException()
        {
            // Arrange
            CommissionFeeDetails commissionFeeDetails = null;

            // Act
            Action actual = () => CommissionFee.CreateNew(commissionFeeDetails);

            // Assertion
            Assert.Throws<ArgumentNullException>(actual);
        }

        [Theory]
        [MemberData(nameof(Input))]
        public void CommissionFeeCreateNewFromDetails_NotNull_ReturnsActualValue(CommissionFeeDetails commissionFeeDetail)
        {
            // Arrange

            // Act
            var commissionFee = CommissionFee.CreateNew(commissionFeeDetail);

            // Assertion
            Assert.Equal(commissionFeeDetail.Name, commissionFee.Name);
            Assert.Equal(commissionFeeDetail.Description, commissionFee.Description);
            Assert.Equal(commissionFeeDetail.Type, commissionFee.Type);
            Assert.Equal(commissionFeeDetail.CalculationType, commissionFee.CalculationType);
            Assert.Equal(commissionFeeDetail.Fee, commissionFee.Fee);
            Assert.Equal(commissionFeeDetail.Priority, commissionFee.Priority);
        }

        [Fact]
        public void CommissionFeeUpdateFromDetails_Null_ThrowsArgumentNullException()
        {
            // Arrange
            CommissionFee commissionFee = new CommissionFee();
            CommissionFeeDetails commissionFeeDetails = null;

            // Act
            Action actual = () => commissionFee.Update(commissionFeeDetails);

            // Assertion
            Assert.Throws<ArgumentNullException>(actual);
        }

        [Theory]
        [MemberData(nameof(Input))]
        public void CommissionFeeUpdateFromDetails_NotNull_ReturnsActualValue(CommissionFeeDetails commissionFeeDetail)
        {
            // Arrange

            // Act
            var commissionFee = new CommissionFee();
            commissionFee.Update(commissionFeeDetail);

            // Assertion
            Assert.Equal(commissionFeeDetail.Name, commissionFee.Name);
            Assert.Equal(commissionFeeDetail.Description, commissionFee.Description);
            Assert.Equal(commissionFeeDetail.Type, commissionFee.Type);
            Assert.Equal(commissionFeeDetail.CalculationType, commissionFee.CalculationType);
            Assert.Equal(commissionFeeDetail.Fee, commissionFee.Fee);
            Assert.Equal(commissionFeeDetail.Priority, commissionFee.Priority);
        }

        [Fact]
        public void CommissionFeeSetDefault_Set_ReturnsTrue()
        {
            // Arrange
            var commissionFee = new CommissionFee { Id = "NewDefaultFeeId", IsDefault = false };
            var previousDefaultCommissionFee = new CommissionFee { Id = "PreviousDefaultFeeId", IsDefault = true };

            // Act
            commissionFee.SetAsDefault(previousDefaultCommissionFee);

            // Assertion
            Assert.True(commissionFee.IsDefault);
            Assert.False(previousDefaultCommissionFee.IsDefault);
        }

        [Fact]
        public void CommissionFeeGetAmount_NegativePrice_ThrowsArgumentNullException()
        {
            // Arrange
            var commissionFee = new CommissionFee
            {
                CalculationType = FeeCalculationType.Fixed,
                Fee = 10
            };
            decimal price = -1;

            // Act
            Action actual = () => commissionFee.GetFeeAmount(price);

            // Assertion
            Assert.Throws<ArgumentNullException>(actual);
        }

        [Theory]
        [InlineData(0, FeeCalculationType.Fixed, 0, 0)]
        [InlineData(10, FeeCalculationType.Fixed, 5, 5)]
        [InlineData(10, FeeCalculationType.Fixed, 50, 10)]
        [InlineData(0, FeeCalculationType.Percent, 10, 0)]
        [InlineData(10, FeeCalculationType.Percent, 10, 1)]
        public void CommissionFeeGetAmount_PositivePrice_ReturnsExpected(decimal price, FeeCalculationType feeCalculationType, decimal fee, decimal expectedFee)
        {
            // Arrange
            var commissionFee = new CommissionFee
            {
                CalculationType = feeCalculationType,
                Fee = fee
            };

            // Act
            decimal calculatedFee = commissionFee.GetFeeAmount(price);

            // Assertion
            Assert.Equal(expectedFee, calculatedFee);
        }

        public static TheoryData<CommissionFeeDetails> Input()
        {
            return new TheoryData<CommissionFeeDetails>()
            {
                new CommissionFeeDetails
                {
                    Id = "CommissionFeeTestId",
                    Name = "My test commission details",
                    Description = "My test commission details description",
                    Type = CommissionFeeType.Static,
                    CalculationType = FeeCalculationType.Fixed,
                    Fee = 1,
                    Priority = 0,
                    IsActive = true,
                    IsDefault = true,
                    ExpressionTree = null
                }
            };
        }
    }
}
