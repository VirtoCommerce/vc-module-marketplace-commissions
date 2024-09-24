using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Models;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.MySql
{
    public class CommissionFeeEntityConfiguration : IEntityTypeConfiguration<CommissionFeeEntity>
    {
        public void Configure(EntityTypeBuilder<CommissionFeeEntity> builder)
        {
            builder.Property(x => x.Fee).HasColumnType("decimal").HasPrecision(18, 4);
        }
    }
}
