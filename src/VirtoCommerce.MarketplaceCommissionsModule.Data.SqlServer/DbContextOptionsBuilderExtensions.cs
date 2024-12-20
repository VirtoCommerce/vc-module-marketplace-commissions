using Microsoft.EntityFrameworkCore;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.SqlServer
{
    public static class DbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Configures the context to use SqlServer.
        /// </summary>
        public static DbContextOptionsBuilder UseSqlServerDatabase(this DbContextOptionsBuilder builder, string connectionString)
        {
            return builder.UseSqlServer(connectionString, db => db
                .MigrationsAssembly(typeof(SqlServerDbContextFactory).Assembly.GetName().Name));
        }
    }
}
