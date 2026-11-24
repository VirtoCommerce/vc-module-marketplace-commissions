using System.Collections.Generic;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
public class SearchSellerCommissionsQuery : SearchCriteriaBase, IQuery<SearchSellerCommissionsResult>
{
    /// <summary>
    /// Only the vendors assigned to this commission fee
    /// </summary>
    public string CommissionFeeId { get; set; }

    /// <summary>
    /// Only these vendors
    /// </summary>
    public IList<string> SellerIds { get; set; }
}
