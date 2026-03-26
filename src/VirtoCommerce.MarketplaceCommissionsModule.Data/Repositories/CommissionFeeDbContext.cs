using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Models;
using VirtoCommerce.Platform.Data.Infrastructure;
using CommissionFeeEntity = VirtoCommerce.MarketplaceCommissionsModule.Data.Models.CommissionFeeEntity;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Repositories;

public class CommissionFeeDbContext : DbContextBase
{
    public CommissionFeeDbContext(DbContextOptions<CommissionFeeDbContext> options)
        : base(options)
    {
    }

    protected CommissionFeeDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // EF Core 9 introduced PendingModelChangesWarning which throws when migrations
        // were generated with an older EF Core version. Use reflection to get the EventId
        // since this module is compiled against EF Core 8.
        var pendingField = typeof(RelationalEventId)
            .GetField("PendingModelChangesWarning", BindingFlags.Public | BindingFlags.Static);

        if (pendingField != null)
        {
            var eventId = (EventId)pendingField.GetValue(null);
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(eventId));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region CommissionFeeEntity
        modelBuilder.Entity<CommissionFeeEntity>().ToTable("CommissionFee").HasKey(x => x.Id);
        modelBuilder.Entity<CommissionFeeEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
        modelBuilder.Entity<CommissionFeeEntity>().Property(x => x.Type)
        .HasConversion(y => y.ToString(), y => (CommissionFeeType)Enum.Parse(typeof(CommissionFeeType), y));
        modelBuilder.Entity<CommissionFeeEntity>().Property(x => x.CalculationType)
        .HasConversion(y => y.ToString(), y => (FeeCalculationType)Enum.Parse(typeof(FeeCalculationType), y));
        #endregion

        #region SellerCommissionEntity
        modelBuilder.Entity<SellerCommissionEntity>().ToTable("SellerCommission").HasKey(x => x.Id);
        modelBuilder.Entity<SellerCommissionEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
        modelBuilder.Entity<SellerCommissionEntity>().HasOne(x => x.Seller).WithMany()
                    .HasForeignKey(x => x.SellerId).IsRequired().OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<SellerCommissionEntity>().HasOne(x => x.CommissionFee).WithMany()
                    .HasForeignKey(x => x.CommissionFeeId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        #endregion

        base.OnModelCreating(modelBuilder);

        switch (Database.ProviderName)
        {
            case "Pomelo.EntityFrameworkCore.MySql":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.MarketplaceCommissionsModule.Data.MySql"));
                break;
            case "Npgsql.EntityFrameworkCore.PostgreSQL":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.MarketplaceCommissionsModule.Data.PostgreSql"));
                break;
            case "Microsoft.EntityFrameworkCore.SqlServer":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.MarketplaceCommissionsModule.Data.SqlServer"));
                break;
        }

    }
}
