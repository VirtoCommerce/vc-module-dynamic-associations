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
    public class AssociationService : IAssociationService
    {
        private readonly Func<IAssociationsRepository> _associationsRepositoryFactory;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IEventPublisher _eventPublisher;

        public AssociationService(
            Func<IAssociationsRepository> associationsRepositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher)
        {
            _associationsRepositoryFactory = associationsRepositoryFactory;
            _platformMemoryCache = platformMemoryCache;
            _eventPublisher = eventPublisher;
        }

        public async virtual Task<Association[]> GetByIdsAsync(string[] itemIds)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(GetByIdsAsync), string.Join("-", itemIds.OrderBy(x => x)));

            var result = await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                var rules = Array.Empty<Association>();

                if (!itemIds.IsNullOrEmpty())
                {
                    using (var dynamicAssociationsRepository = _associationsRepositoryFactory())
                    {
                        //Optimize performance and CPU usage
                        dynamicAssociationsRepository.DisableChangesTracking();

                        var entities = await dynamicAssociationsRepository.GetAssociationsByIdsAsync(itemIds);

                        rules = entities
                            .Select(x => x.ToModel(AbstractTypeFactory<Association>.TryCreateInstance()))
                            .ToArray();

                        cacheEntry.AddExpirationToken(AssociationCacheRegion.CreateChangeToken(itemIds));
                    }
                }

                return rules;
            });

            return result;
        }

        public async virtual Task SaveChangesAsync(Association[] items)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<Association>>();

            using (var dynamicAssociationsRepository = _associationsRepositoryFactory())
            {
                var ids = items.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray();
                var dbExistProducts = await dynamicAssociationsRepository.GetAssociationsByIdsAsync(ids);

                foreach (var association in items)
                {
                    var modifiedEntity = AbstractTypeFactory<AssociationEntity>.TryCreateInstance().FromModel(association, pkMap);
                    var originalEntity = dbExistProducts.FirstOrDefault(x => x.Id == association.Id);

                    if (originalEntity != null)
                    {
                        changedEntries.Add(new GenericChangedEntry<Association>(association, originalEntity.ToModel(AbstractTypeFactory<Association>.TryCreateInstance()), EntryState.Modified));
                        modifiedEntity.Patch(originalEntity);
                    }
                    else
                    {
                        dynamicAssociationsRepository.Add(modifiedEntity);
                        changedEntries.Add(new GenericChangedEntry<Association>(association, EntryState.Added));
                    }
                }

                await _eventPublisher.Publish(new AssociationChangingEvent(changedEntries));

                await dynamicAssociationsRepository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();

                await _eventPublisher.Publish(new AssociationChangedEvent(changedEntries));
            }

            ClearCache(items);
        }

        public async virtual Task DeleteAsync(string[] itemIds)
        {
            var items = await GetByIdsAsync(itemIds);
            var changedEntries = items
                .Select(x => new GenericChangedEntry<Association>(x, EntryState.Deleted))
                .ToArray();

            using (var associationsRepositoryFactory = _associationsRepositoryFactory())
            {
                await _eventPublisher.Publish(new AssociationChangingEvent(changedEntries));

                var associationEntities = await associationsRepositoryFactory.GetAssociationsByIdsAsync(itemIds);

                foreach (var associationEntity in associationEntities)
                {
                    associationsRepositoryFactory.Remove(associationEntity);
                }

                await associationsRepositoryFactory.UnitOfWork.CommitAsync();

                await _eventPublisher.Publish(new AssociationChangedEvent(changedEntries));
            }

            ClearCache(items);
        }


        protected virtual void ClearCache(IEnumerable<Association> associations)
        {
            foreach (var association in associations)
            {
                AssociationCacheRegion.ExpireEntity(association);
            }

            AssociationSearchCacheRegion.ExpireRegion();
        }
    }
}
