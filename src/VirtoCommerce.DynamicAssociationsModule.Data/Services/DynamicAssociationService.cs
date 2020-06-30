using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.DynamicAssociationsModule.Core.Events;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Services;
using VirtoCommerce.DynamicAssociationsModule.Data.Caching;
using VirtoCommerce.DynamicAssociationsModule.Data.Model;
using VirtoCommerce.DynamicAssociationsModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Services
{
    public class DynamicAssociationService : IDynamicAssociationService
    {
        private readonly Func<IDynamicAssociationsRepository> _dynamicAssociationsRepositoryFactory;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IEventPublisher _eventPublisher;

        public DynamicAssociationService(
            Func<IDynamicAssociationsRepository> dynamicAssociationsRepositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher)
        {
            _dynamicAssociationsRepositoryFactory = dynamicAssociationsRepositoryFactory;
            _platformMemoryCache = platformMemoryCache;
            _eventPublisher = eventPublisher;
        }

        public async virtual Task<DynamicAssociation[]> GetByIdsAsync(string[] itemIds)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(GetByIdsAsync), string.Join("-", itemIds.OrderBy(x => x)));

            var result = await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                var rules = Array.Empty<DynamicAssociation>();

                if (!itemIds.IsNullOrEmpty())
                {
                    using (var dynamicAssociationsRepository = _dynamicAssociationsRepositoryFactory())
                    {
                        //Optimize performance and CPU usage
                        dynamicAssociationsRepository.DisableChangesTracking();

                        var entities = await dynamicAssociationsRepository.GetDynamicAssociationsByIdsAsync(itemIds);

                        rules = entities
                            .Select(x => x.ToModel(AbstractTypeFactory<DynamicAssociation>.TryCreateInstance()))
                            .ToArray();

                        cacheEntry.AddExpirationToken(DynamicAssociationCacheRegion.CreateChangeToken(itemIds));
                    }
                }

                return rules;
            });

            return result;
        }

        public async virtual Task SaveChangesAsync(DynamicAssociation[] items)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<DynamicAssociation>>();

            using (var dynamicAssociationsRepository = _dynamicAssociationsRepositoryFactory())
            {
                var ids = items.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray();
                var dbExistProducts = await dynamicAssociationsRepository.GetDynamicAssociationsByIdsAsync(ids);

                foreach (var dynamicAssociation in items)
                {
                    var modifiedEntity = AbstractTypeFactory<DynamicAssociationEntity>.TryCreateInstance().FromModel(dynamicAssociation, pkMap);
                    var originalEntity = dbExistProducts.FirstOrDefault(x => x.Id == dynamicAssociation.Id);

                    if (originalEntity != null)
                    {
                        changedEntries.Add(new GenericChangedEntry<DynamicAssociation>(dynamicAssociation, originalEntity.ToModel(AbstractTypeFactory<DynamicAssociation>.TryCreateInstance()), EntryState.Modified));
                        modifiedEntity.Patch(originalEntity);
                    }
                    else
                    {
                        dynamicAssociationsRepository.Add(modifiedEntity);
                        changedEntries.Add(new GenericChangedEntry<DynamicAssociation>(dynamicAssociation, EntryState.Added));
                    }
                }

                await _eventPublisher.Publish(new DynamicAssociationChangingEvent(changedEntries));

                await dynamicAssociationsRepository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();

                await _eventPublisher.Publish(new DynamicAssociationChangedEvent(changedEntries));
            }

            ClearCache(items);
        }

        public async virtual Task DeleteAsync(string[] itemIds)
        {
            var items = await GetByIdsAsync(itemIds);
            var changedEntries = items
                .Select(x => new GenericChangedEntry<DynamicAssociation>(x, EntryState.Deleted))
                .ToArray();

            using (var dynamicAssociationsRepository = _dynamicAssociationsRepositoryFactory())
            {
                await _eventPublisher.Publish(new DynamicAssociationChangingEvent(changedEntries));

                var dynamicAssociationEntities = await dynamicAssociationsRepository.GetDynamicAssociationsByIdsAsync(itemIds);

                foreach (var dynamicAssociationEntity in dynamicAssociationEntities)
                {
                    dynamicAssociationsRepository.Remove(dynamicAssociationEntity);
                }

                await dynamicAssociationsRepository.UnitOfWork.CommitAsync();

                await _eventPublisher.Publish(new DynamicAssociationChangedEvent(changedEntries));
            }

            ClearCache(items);
        }


        protected virtual void ClearCache(IEnumerable<DynamicAssociation> dynamicAssociations)
        {
            foreach (var dynamicAssociation in dynamicAssociations)
            {
                DynamicAssociationCacheRegion.ExpireEntity(dynamicAssociation);
            }

            DynamicAssociationSearchCacheRegion.ExpireRegion();
        }
    }
}
