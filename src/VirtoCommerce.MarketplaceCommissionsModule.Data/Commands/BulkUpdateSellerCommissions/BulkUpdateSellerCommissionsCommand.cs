using System.Collections.Generic;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Commands
{
    /// <summary>
    /// Assigns vendors of one static commission fee to another static commission fee
    /// </summary>
    public class BulkUpdateSellerCommissionsCommand : ICommand<BulkUpdateSellerCommissionsResult>
    {
        /// <summary>
        /// The static commission fee the vendors are assigned to now; vendors assigned elsewhere are skipped
        /// </summary>
        public string CommissionFeeId { get; set; }

        /// <summary>
        /// The static commission fee to assign the vendors to
        /// </summary>
        public string TargetCommissionFeeId { get; set; }

        /// <summary>
        /// The vendors to update, when not <see cref="AllSellers"/>
        /// </summary>
        public IList<string> SellerIds { get; set; }

        /// <summary>
        /// Update all vendors of <see cref="CommissionFeeId"/> that match <see cref="Keyword"/>, except <see cref="ExcludedSellerIds"/>
        /// </summary>
        public bool AllSellers { get; set; }

        /// <summary>
        /// Vendor name filter, with <see cref="AllSellers"/>
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// The vendors to leave unchanged, with <see cref="AllSellers"/>
        /// </summary>
        public IList<string> ExcludedSellerIds { get; set; }
    }
}
