using Microsoft.Extensions.DependencyInjection;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests
{
    public static class TestHepler
    {
        public static IServiceCollection AddCollection<T>(this IServiceCollection services, T t) where T : class
        {
            return services.AddTransient(provider => t);
        }
    }
}
