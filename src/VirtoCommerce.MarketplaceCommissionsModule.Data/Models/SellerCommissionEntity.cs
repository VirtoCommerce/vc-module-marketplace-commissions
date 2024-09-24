using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceVendorModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Models
{
    public class SellerCommissionEntity : AuditableEntity, IDataEntity<SellerCommissionEntity, SellerCommission>
    {
        #region Navigation properties

        [StringLength(128)]
        [Required]
        public string SellerId { get; set; }

        public SellerEntity Seller { get; set; }

        [StringLength(128)]
        [Required]
        public string CommissionFeeId { get; set; }

        public CommissionFeeEntity CommissionFee { get; set; }

        #endregion

        public virtual SellerCommissionEntity FromModel(SellerCommission model, PrimaryKeyResolvingMap pkMap)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            pkMap.AddPair(model, this);

            Id = model.Id;
            CreatedBy = model.CreatedBy;
            CreatedDate = model.CreatedDate;
            ModifiedBy = model.ModifiedBy;
            ModifiedDate = model.ModifiedDate;

            SellerId = model.SellerId;
            CommissionFeeId = model.CommissionFeeId;

            return this;
        }

        public virtual void Patch(SellerCommissionEntity target)
        {
            target.SellerId = SellerId;
            target.CommissionFeeId = CommissionFeeId;
        }

        public virtual SellerCommission ToModel(SellerCommission model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            model.Id = Id;
            model.CreatedBy = CreatedBy;
            model.CreatedDate = CreatedDate;
            model.ModifiedBy = ModifiedBy;
            model.ModifiedDate = ModifiedDate;

            model.SellerId = SellerId;
            model.CommissionFeeId = CommissionFeeId;

            return model;
        }

    }
}
