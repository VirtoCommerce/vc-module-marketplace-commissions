using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public void CommissionFeeCreateNewFromDetails_NotNull_ReturnsActualValue()
        {
            // Arrange
            var commissionFeeDetails = TestHepler.LoadFromJsonFile<CommissionFeeDetails[]>(@"commissionFeeDetails.json");
            var staticCommissionFeeDetails = commissionFeeDetails.Where(x => x.Type == CommissionFeeType.Static).ToArray();
            var commissionFees = new List<CommissionFee>();
            int index = 0;

            // Act
            foreach (var commissionFeeDetail in staticCommissionFeeDetails)
            {
                var commissionFee = CommissionFee.CreateNew(commissionFeeDetail);
                commissionFees.Add(commissionFee);
            }

            // Assertion
            foreach (var commissionFeeDetail in staticCommissionFeeDetails)
            {
                var commissionFee = commissionFees[index++];

                Assert.Equal(commissionFeeDetail.Name, commissionFee.Name);
                Assert.Equal(commissionFeeDetail.Description, commissionFee.Description);
                Assert.Equal(commissionFeeDetail.Type, commissionFee.Type);
                Assert.Equal(commissionFeeDetail.CalculationType, commissionFee.CalculationType);
                Assert.Equal(commissionFeeDetail.Fee, commissionFee.Fee);
                Assert.Equal(commissionFeeDetail.Priority, commissionFee.Priority);
            }
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

        [Fact]
        public void CommissionFeeUpdateFromDetails_NotNull_ReturnsActualValue()
        {
            // Arrange
            var commissionFeeDetails = TestHepler.LoadFromJsonFile<CommissionFeeDetails[]>(@"commissionFeeDetails.json");
            var staticCommissionFeeDetails = commissionFeeDetails.Where(x => x.Type == CommissionFeeType.Static).ToArray();
            var commissionFees = new List<CommissionFee>();
            int index = 0;

            // Act
            foreach (var commissionFeeDetail in staticCommissionFeeDetails)
            {
                var commissionFee = new CommissionFee();
                commissionFee.Update(commissionFeeDetail);
                commissionFees.Add(commissionFee);
            }

            // Assertion
            foreach (var commissionFeeDetail in staticCommissionFeeDetails)
            {
                var commissionFee = commissionFees[index++];

                Assert.Equal(commissionFeeDetail.Name, commissionFee.Name);
                Assert.Equal(commissionFeeDetail.Description, commissionFee.Description);
                Assert.Equal(commissionFeeDetail.Type, commissionFee.Type);
                Assert.Equal(commissionFeeDetail.CalculationType, commissionFee.CalculationType);
                Assert.Equal(commissionFeeDetail.Fee, commissionFee.Fee);
                Assert.Equal(commissionFeeDetail.Priority, commissionFee.Priority);
            }
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
    }
}
