using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Search;
using VirtoCommerce.DynamicAssociationsModule.Core.Search;
using VirtoCommerce.DynamicAssociationsModule.Core.Services;
using VirtoCommerce.DynamicAssociationsModule.Data.Caching;
using VirtoCommerce.DynamicAssociationsModule.Data.Model;
using VirtoCommerce.DynamicAssociationsModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Search
{
    public class AssociationSearchService : IAssociationSearchService
    {
        private readonly Func<IAssociationsRepository> _associationsRepositoryFactory;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IAssociationService _associationService;

        public AssociationSearchService(Func<IAssociationsRepository> associationsRepositoryFactory, IPlatformMemoryCache platformMemoryCache, IAssociationService associationService)
        {
            _associationsRepositoryFactory = associationsRepositoryFactory;
            _platformMemoryCache = platformMemoryCache;
            _associationService = associationService;
        }

        public async virtual Task<AssociationSearchResult> SearchAssociationsAsync(AssociationSearchCriteria criteria)
        {
            ValidateParameters(criteria);

            var cacheKey = CacheKey.With(GetType(), nameof(SearchAssociationsAsync), criteria.GetCacheKey());

            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(AssociationSearchCacheRegion.CreateChangeToken());

                var result = AbstractTypeFactory<AssociationSearchResult>.TryCreateInstance();

                using (var associationsRepositoryFactory = _associationsRepositoryFactory())
                {
                    //Optimize performance and CPU usage
                    associationsRepositoryFactory.DisableChangesTracking();
                    var sortInfos = BuildSortExpression(criteria);
                    var query = BuildQuery(associationsRepositoryFactory, criteria);

                    result.TotalCount = await query.CountAsync();

                    if (criteria.Take > 0 && result.TotalCount > 0)
                    {
                        var ids = await query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id)
                            .Select(x => x.Id)
                            .Skip(criteria.Skip).Take(criteria.Take)
                            .AsNoTracking()
                            .ToArrayAsync();

                        result.Results = (await _associationService.GetByIdsAsync(ids)).OrderBy(x => Array.IndexOf(ids, x.Id)).ToList();
                    }
                }

                return result;
            });
        }

        protected virtual IQueryable<AssociationEntity> BuildQuery(IAssociationsRepository repository, AssociationSearchCriteria criteria)
        {
            var query = repository.Associations;

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword));
            }

            if (!criteria.StoreIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.StoreIds.Contains(x.StoreId));
            }

            if (!criteria.Groups.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Groups.Contains(x.AssociationType));
            }

            if (criteria.IsActive != null)
            {
                var utcNow = DateTime.UtcNow;
                query = query.Where(x => x.IsActive == criteria.IsActive && (x.StartDate == null || utcNow >= x.StartDate) && (x.EndDate == null || x.EndDate >= utcNow));
            }

            return query;
        }

        protected virtual IList<SortInfo> BuildSortExpression(AssociationSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;

            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[] { new SortInfo { SortColumn = nameof(Association.Name) } };
            }

            return sortInfos;
        }

        private static void ValidateParameters(AssociationSearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }
        }
    }
}
