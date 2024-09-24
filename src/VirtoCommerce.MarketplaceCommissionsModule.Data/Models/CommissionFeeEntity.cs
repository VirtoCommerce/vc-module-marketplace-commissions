using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.JsonConverters;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Models
{
    public class CommissionFeeEntity : AuditableEntity, IDataEntity<CommissionFeeEntity, CommissionFee>
    {
        [StringLength(254)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [StringLength(128)]
        public CommissionFeeType Type { get; set; }

        [StringLength(128)]
        public FeeCalculationType CalculationType { get; set; }

        [Column(TypeName = "Money")]
        public decimal Fee { get; set; }

        public int Priority { get; set; }
        public bool IsActive { get; set; }

        public bool IsDefault { get; set; }

        public string PredicateVisualTreeSerialized { get; set; }

        public virtual CommissionFeeEntity FromModel(CommissionFee model, PrimaryKeyResolvingMap pkMap)
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

            Name = model.Name;
            Description = model.Description;
            Type = model.Type;
            CalculationType = model.CalculationType;
            Fee = model.Fee;
            IsDefault = model.IsDefault;
            Priority = model.Priority;

            if (model is DynamicCommissionFee dynamicCommissionFee)
            {
                IsActive = dynamicCommissionFee.IsActive;
                if (dynamicCommissionFee.ExpressionTree != null)
                {
                    PredicateVisualTreeSerialized = JsonConvert.SerializeObject(dynamicCommissionFee.ExpressionTree, new ConditionJsonConverter(doNotSerializeAvailCondition: true));
                }
            }

            return this;
        }

        public virtual void Patch(CommissionFeeEntity target)
        {
            target.Name = Name;
            target.Description = Description;
            target.Type = Type;
            target.CalculationType = CalculationType;
            target.Fee = Fee;
            target.Priority = Priority;
            target.IsActive = IsActive;
            target.IsDefault = IsDefault;
            target.PredicateVisualTreeSerialized = PredicateVisualTreeSerialized;
        }

        public virtual CommissionFee ToModel(CommissionFee model)
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

            model.Name = Name;
            model.Description = Description;
            model.Type = Type;
            model.CalculationType = CalculationType;
            model.Fee = Fee;
            model.IsDefault = IsDefault;
            model.Priority = Priority;

            if (model is DynamicCommissionFee dynamicCommissionFee)
            {
                dynamicCommissionFee.IsActive = IsActive;
                dynamicCommissionFee.ExpressionTree = ExType<DynamicCommissionFeeTree>.New();
                if (PredicateVisualTreeSerialized != null)
                {
                    dynamicCommissionFee.ExpressionTree = JsonConvert.DeserializeObject<DynamicCommissionFeeTree>(PredicateVisualTreeSerialized, new ConditionJsonConverter(), new PolymorphJsonConverter());
                }
            }

            return model;
        }
    }
}
