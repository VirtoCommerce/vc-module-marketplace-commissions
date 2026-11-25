namespace VirtoCommerce.MarketplaceCommissionsModule.Data.BackgroundJobs
{
    /// <summary>
    /// Payload of <see cref="CommissionFeesJobHandler"/>: the ids of the new customer orders to apply commission fees to.
    /// </summary>
    public class CommissionFeesJobPayload
    {
        public string[] OrderIds { get; set; }
    }
}
