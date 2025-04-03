using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Repositories;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Services;
using VirtoCommerce.MarketplaceVendorModule.Data.Integrations;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Functional
{
    public class ServiceBuilder
    {
        public ServiceCollection GetServiceCollection()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            services.AddTransient<ILoggerFactory, LoggerFactory>();
            services.AddTransient<ISellerResolver, SellerResolver>();
            services.AddTransient<ICommissionFeeRepository, CommissionFeeRepositoryMock>();
            services.AddTransient<ICommissionFeeEvaluator, CommissionFeeEvaluator>();
            services.AddTransient<ICommissionFeeSearchService, CommissionFeeSearchServiceStub>();
            services.AddTransient<ICommissionFeeService, CommissionFeeServiceMock>();
            services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<Anchor>());
            return services;
        }
    }
}
