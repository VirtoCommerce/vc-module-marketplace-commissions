using System;
using System.Collections.Generic;
using System.Linq;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public static class ComissionFeeConditionsExtensions
    {
        public static IEnumerable<EntryFee> InCategories(this IEnumerable<EntryFee> entries, IEnumerable<string> categoryIds)
        {
            categoryIds = categoryIds.Where(x => x != null);
            return categoryIds.Any() ? entries.Where(x => IsEntryInCategories(x, categoryIds)) : entries;
        }

        public static bool IsEntryInCategories(this EntryFee entry, IEnumerable<string> categoryIds)
        {
            var result = categoryIds.Contains(entry.CategoryId, StringComparer.OrdinalIgnoreCase);
            if (!result && entry.Outline != null)
            {
                result = entry.Outline.Split(';', '/', '\\').Intersect(categoryIds, StringComparer.OrdinalIgnoreCase).Any();
            }
            return result;
        }

        public static IEnumerable<EntryFee> ExcludeCategories(this IEnumerable<EntryFee> entries, IEnumerable<string> categoryIds)
        {
            var result = entries.Where(x => !IsEntryInCategories(x, categoryIds));
            return result;
        }

        public static IEnumerable<EntryFee> ExcludeProducts(this IEnumerable<EntryFee> entries, IEnumerable<string> productIds)
        {
            var result = entries.Where(x => !IsEntryInProducts(x, productIds));
            return result;
        }

        public static bool IsEntryInProducts(this EntryFee entry, IEnumerable<string> productIds)
        {
            return productIds.Contains(entry.ProductId, StringComparer.OrdinalIgnoreCase);
        }
    }
}
