using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Commands;
using Xunit;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Functional
{
    public class CommissionFeeIsDefaultTests
    {
        [Fact]
        public async Task Create_new_fee()
        {
            // Arrange
            var mediator = GetMediator(new CommissionFeeServiceMock());

            // Act
            var fee = await mediator.Send(new CreateFeeCommand
            {
                FeeDetails = new CommissionFeeDetails { IsDefault = true }
            });

            await mediator.Send(new CreateFeeCommand
            {
                FeeDetails = new CommissionFeeDetails { IsDefault = true }
            });

            // Assertion
            fee.IsDefault.Should().BeFalse();
        }

        [Fact]
        public async Task Update_fee()
        {
            // Arrange
            var mediator = GetMediator(new CommissionFeeServiceMock());

            // Act
            var fee = await mediator.Send(new CreateFeeCommand
            {
                FeeDetails = new CommissionFeeDetails
                {
                    IsDefault = true
                }
            });

            await mediator.Send(new UpdateFeeCommand
            {
                FeeDetails = new CommissionFeeDetails
                {
                    Id = fee.Id,
                    IsDefault = false
                }
            });

            // Assertion
            fee.IsDefault.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_fee()
        {
            // Arrange
            var feeServiceMock = new CommissionFeeServiceMock();
            var mediator = GetMediator(feeServiceMock);

            // Act
            var fee = await mediator.Send(new CreateFeeCommand
            {
                FeeDetails = new CommissionFeeDetails
                {
                    IsDefault = false
                }
            });

            await mediator.Send(new DeleteFeeCommand { Id = fee.Id });

            // Assertion
            feeServiceMock.CommissionFees.Count.Should().Be(0);
        }

        public IMediator GetMediator(CommissionFeeServiceMock feeServiceMock)
        {
            var serviceProvider = new ServiceBuilder().GetServiceCollection()
                .AddCollection<ICommissionFeeService>(feeServiceMock)
                .BuildServiceProvider();
            return serviceProvider.GetService<IMediator>();
        }
    }
}
