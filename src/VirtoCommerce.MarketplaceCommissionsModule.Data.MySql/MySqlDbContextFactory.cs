using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Repositories;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.MySql
{
    public class MySqlDbContextFactory : IDesignTimeDbContextFactory<CommissionFeeDbContext>
    {
        public CommissionFeeDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CommissionFeeDbContext>();
            var connectionString = args.Any() ? args[0] : "server=localhost;user=root;password=virto;database=VirtoCommerce3;";
            var serverVersion = args.Length >= 2 ? args[1] : null;

            builder.UseMySql(
                connectionString,
                ResolveServerVersion(serverVersion, connectionString),
                db => db.MigrationsAssembly(typeof(MySqlDbContextFactory).Assembly.GetName().Name));

            return new CommissionFeeDbContext(builder.Options);
        }

        private static ServerVersion ResolveServerVersion(string? serverVersion, string connectionString)
        {
            if (serverVersion == "AutoDetect")
            {
                return ServerVersion.AutoDetect(connectionString);
            }

            if (serverVersion != null)
            {
                return ServerVersion.Parse(serverVersion);
            }

            return new MySqlServerVersion(new Version(5, 7));
        }
    }
}
