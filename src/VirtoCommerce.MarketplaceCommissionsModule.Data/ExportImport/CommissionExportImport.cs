using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.ExportImport;
public class CommissionExportImport
{
    private readonly ICommissionFeeSearchService _commissionFeeSearchService;
    private readonly ICommissionFeeService _commissionFeeCrudService;
    private readonly ISellerCommissionSearchService _sellerCommissionSearchService;
    private readonly ISellerCommissionCrudService _sellerCommissionCrudService;
    private readonly JsonSerializer _jsonSerializer;
    private readonly int _batchSize = 50;

    public CommissionExportImport(
        ICommissionFeeSearchService commissionFeeSearchService,
        ICommissionFeeService commissionFeeCrudService,
        ISellerCommissionSearchService sellerCommissionSearchService,
        ISellerCommissionCrudService sellerCommissionCrudService,
        JsonSerializer jsonSerializer
        )
    {
        _commissionFeeSearchService = commissionFeeSearchService;
        _commissionFeeCrudService = commissionFeeCrudService;
        _sellerCommissionSearchService = sellerCommissionSearchService;
        _sellerCommissionCrudService = sellerCommissionCrudService;
        _jsonSerializer = jsonSerializer;
    }

    public virtual async Task DoExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var progressInfo = new ExportImportProgressInfo { Description = "loading data..." };
        progressCallback(progressInfo);

        using (var sw = new StreamWriter(outStream))
        using (var writer = new JsonTextWriter(sw))
        {
            await writer.WriteStartObjectAsync();

            #region Export CommissionFees

            progressInfo.Description = "CommissionFees exporting...";
            progressCallback(progressInfo);

            await writer.WritePropertyNameAsync("CommissionFees");
            await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
            {
                var searchResult = await _commissionFeeSearchService.SearchAsync(new SearchCommissionFeesQuery { Skip = skip, Take = take });
                return (GenericSearchResult<CommissionFee>)searchResult;
            }
            , (processedCount, totalCount) =>
            {
                progressInfo.Description = $"{processedCount} of {totalCount} fees have been exported";
                progressCallback(progressInfo);
            }, cancellationToken);

            #endregion

            #region Export SellerCommissions

            progressInfo.Description = "SellerCommissions exporting...";
            progressCallback(progressInfo);

            await writer.WritePropertyNameAsync("SellerCommissions");
            await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
            {
                var searchResult = await _sellerCommissionSearchService.SearchAsync(new SearchSellerCommissionsQuery { Skip = skip, Take = take });
                return (GenericSearchResult<SellerCommission>)searchResult;
            }
            , (processedCount, totalCount) =>
            {
                progressInfo.Description = $"{processedCount} of {totalCount} seller commissions have been exported";
                progressCallback(progressInfo);
            }, cancellationToken);

            #endregion

            await writer.WriteEndObjectAsync();
            await writer.FlushAsync();
        }
    }

    public virtual async Task DoImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var progressInfo = new ExportImportProgressInfo();

        using (var streamReader = new StreamReader(inputStream))
        using (var reader = new JsonTextReader(streamReader))
        {
            while (await reader.ReadAsync())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                switch (reader.Value?.ToString())
                {
                    case "CommissionFees":
                        await reader.DeserializeArrayWithPagingAsync<CommissionFee>(_jsonSerializer, _batchSize, items => _commissionFeeCrudService.SaveChangesAsync(items), processedCount =>
                        {
                            progressInfo.Description = $"{processedCount} fees have been imported";
                            progressCallback(progressInfo);
                        }, cancellationToken);
                        break;
                    case "SellerCommissions":
                        await reader.DeserializeArrayWithPagingAsync<SellerCommission>(_jsonSerializer, _batchSize, items => _sellerCommissionCrudService.SaveChangesAsync(items), processedCount =>
                        {
                            progressInfo.Description = $"{processedCount} seller commissions have been imported";
                            progressCallback(progressInfo);
                        }, cancellationToken);
                        break;
                    default:
                        continue;
                }
            }
        }
    }
}
