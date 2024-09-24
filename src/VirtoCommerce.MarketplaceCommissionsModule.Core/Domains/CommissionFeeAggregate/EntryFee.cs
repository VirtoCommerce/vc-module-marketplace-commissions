using VirtoCommerce.MarketplaceVendorModule.Core.Domains;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public class EntryFee : ValueObject, IHasSellerId
    {
        public string EntryId { get; set; }
        public string EntryType { get; set; }

        public string SellerId { get; set; }
        public string SellerName { get; set; }
        public int Quantity { get; set; }
        public string CategoryId { get; set; }
        public string Outline { get; set; }
        public string ProductId { get; set; }

        public decimal Price { get; set; }
        public decimal PriceWithTax { get; set; }

        public CommissionFee CommissionFee { get; set; }
    }
}
