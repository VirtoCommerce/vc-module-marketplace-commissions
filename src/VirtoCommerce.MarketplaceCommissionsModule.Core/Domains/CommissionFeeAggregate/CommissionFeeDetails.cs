namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public class CommissionFeeDetails
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CommissionFeeType Type { get; set; }
        public FeeCalculationType CalculationType { get; set; }
        public decimal Fee { get; set; }
        public int Priority { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DynamicCommissionFeeTree ExpressionTree { get; set; }
    }
}
