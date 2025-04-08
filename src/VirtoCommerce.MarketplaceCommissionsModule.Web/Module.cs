using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.MarketplaceCommissionsModule.Core;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data;
using VirtoCommerce.MarketplaceCommissionsModule.Data.ExportImport;
using VirtoCommerce.MarketplaceCommissionsModule.Data.IntegrationEventHandlers;
using VirtoCommerce.MarketplaceCommissionsModule.Data.MySql;
using VirtoCommerce.MarketplaceCommissionsModule.Data.PostgreSql;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Repositories;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data.SqlServer;
using VirtoCommerce.MarketplaceVendorModule.Core.IntegrationEvents;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.MarketplaceCommissionsModule.Web;

public class Module : IModule, IHasConfiguration, IExportSupport, IImportSupport
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }
    private IApplicationBuilder _appBuilder;

    public void Initialize(IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContext<CommissionFeeDbContext>((_, options) =>
        {
            var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
            var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

            switch (databaseProvider)
            {
                case "MySql":
                    options.UseMySqlDatabase(connectionString);
                    break;
                case "PostgreSql":
                    options.UsePostgreSqlDatabase(connectionString);
                    break;
                default:
                    options.UseSqlServerDatabase(connectionString);
                    break;
            }
        });

        serviceCollection.AddTransient<ICommissionFeeRepository, CommissionFeeRepository>();
        serviceCollection.AddTransient<Func<ICommissionFeeRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<ICommissionFeeRepository>());

        serviceCollection.AddTransient<ICommissionFeeSearchService, CommissionFeeSearchService>();
        serviceCollection.AddTransient<ICommissionFeeService, CommissionFeeService>();
        serviceCollection.AddTransient<ICommissionFeeEvaluator, CommissionFeeEvaluator>();
        serviceCollection.AddTransient<ICommissionFeeResolver, CommissionFeeResolver>();

        serviceCollection.AddTransient<ISellerCommissionCrudService, SellerCommissionCrudService>();
        serviceCollection.AddTransient<ISellerCommissionSearchService, SellerCommissionSearchService>();

        serviceCollection.AddTransient<CustomerOrderCreatedEventHandler>();
        serviceCollection.AddTransient<SellerOrderCreatedIntegrationEventHandler>();
        serviceCollection.AddTransient<SellerCreatedIntegrationEventHandler>();

        serviceCollection.AddTransient<CommissionExportImport>();

        serviceCollection.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<Anchor>());
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        _appBuilder = appBuilder;

        var serviceProvider = appBuilder.ApplicationServices;

        // Register settings
        var settingsRegistrar = serviceProvider.GetRequiredService<ISettingsRegistrar>();
        settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

        // Register permissions
        var permissionsRegistrar = serviceProvider.GetRequiredService<IPermissionsRegistrar>();
        permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "MarketplaceCommissionsModule", ModuleConstants.Security.Permissions.AllPermissions);

        appBuilder.RegisterEventHandler<OrderChangedEvent, CustomerOrderCreatedEventHandler>();
        appBuilder.RegisterEventHandler<SellerOrderCreatedIntegrationEvent, SellerOrderCreatedIntegrationEventHandler>();
        appBuilder.RegisterEventHandler<SellerCreatedIntegrationEvent, SellerCreatedIntegrationEventHandler>();

        // Register condition trees expressions into the AbstractFactory<IConditionTree>
        foreach (var conditionTree in AbstractTypeFactory<DynamicCommissionFeeTreePrototype>.TryCreateInstance().Traverse<IConditionTree>(x => x.AvailableChildren))
        {
            AbstractTypeFactory<IConditionTree>.RegisterType(conditionTree.GetType());
        }

        // Apply migrations
        using var serviceScope = serviceProvider.CreateScope();
        using var dbContext = serviceScope.ServiceProvider.GetRequiredService<CommissionFeeDbContext>();
        dbContext.Database.Migrate();
    }

    public void Uninstall()
    {
        // Nothing to do here
    }

    public Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
    ICancellationToken cancellationToken)
    {
        return _appBuilder.ApplicationServices.GetRequiredService<CommissionExportImport>().DoExportAsync(outStream, options,
            progressCallback, cancellationToken);
    }

    public Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
        ICancellationToken cancellationToken)
    {
        return _appBuilder.ApplicationServices.GetRequiredService<CommissionExportImport>().DoImportAsync(inputStream, options,
            progressCallback, cancellationToken);
    }
}
