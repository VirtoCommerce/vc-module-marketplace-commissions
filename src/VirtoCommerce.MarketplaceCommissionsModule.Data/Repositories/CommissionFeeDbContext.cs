using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
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
