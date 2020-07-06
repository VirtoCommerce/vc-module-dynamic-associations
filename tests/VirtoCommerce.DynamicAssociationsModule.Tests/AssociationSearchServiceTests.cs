using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Search;
using VirtoCommerce.DynamicAssociationsModule.Data.Model;
using VirtoCommerce.DynamicAssociationsModule.Data.Repositories;
using VirtoCommerce.DynamicAssociationsModule.Data.Search;
using VirtoCommerce.DynamicAssociationsModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.DynamicAssociationsModule.Tests
{
    public class AssociationSearchServiceTests
    {
        public AssociationSearchServiceTests()
        {
        }

        public static object[][] ValidEntitiesForIsActive => new object[][]
        {
            new object[]
            {
                new AssociationEntity()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsActive = true,
                    StartDate = null,
                    EndDate = null,
                },
            },
            new object[]
            {
                new AssociationEntity()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsActive = true,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = null,
                },
            },
            new object[]
            {
                new AssociationEntity()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsActive = true,
                    StartDate = null,
                    EndDate = DateTime.UtcNow.AddDays(1),
                },
            },
            new object[]
            {
                new AssociationEntity()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsActive = true,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                },
            },
        };

        public static object[][] NotValidEntitiesForIsActive => new object[][]
        {
            new object[]
            {
                new AssociationEntity()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsActive = false,
                },
            },
            new object[]
            {
                new AssociationEntity()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsActive = true,
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = null,
                }
            },
            new object[]
            {
                new AssociationEntity()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsActive = true,
                    StartDate = null,
                    EndDate = DateTime.UtcNow.AddDays(-1),
                }
            },
            new object[]
            {
                new AssociationEntity()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsActive = true,
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(-1),
                }
            },
        };

        [Theory]
        [MemberData(nameof(ValidEntitiesForIsActive))]
        public async Task SearchDynamicAssociationsAsync_IsActiveConditionValid_FoundResults(AssociationEntity entity)
        {
            // Arrange
            var entities = new[]
            {
                entity
            };
            var searchServiceMock = CreateDynamicAssociationSearchServiceMock(entities);

            // Act
            var searchResult = await searchServiceMock.SearchAssociationsAsync(new AssociationSearchCriteria() { IsActive = true });

            // Assert
            Assert.Equal(1, searchResult.TotalCount);
        }

        [Theory]
        [MemberData(nameof(NotValidEntitiesForIsActive))]
        public async Task SearchDynamicAssociationsAsync_IsActiveConditionNotValid_NoResults(AssociationEntity entity)
        {
            // Arrange
            var entities = new[]
            {
                entity
            };
            var searchServiceMock = CreateDynamicAssociationSearchServiceMock(entities);

            // Act
            var searchResult = await searchServiceMock.SearchAssociationsAsync(new AssociationSearchCriteria() { IsActive = true });

            // Assert
            Assert.Equal(0, searchResult.TotalCount);
        }


        private AssociationSearchService CreateDynamicAssociationSearchServiceMock(IEnumerable<AssociationEntity> entities)
        {
            var dynamicAssociationsRepositoryFactory = CreateRepositoryMock(entities);
            var platformMemoryCache = GetPlatformMemoryCache();
            var eventPublisherMock = new Mock<IEventPublisher>();

            var dynamicAssociationsService = new AssociationService(dynamicAssociationsRepositoryFactory, platformMemoryCache, eventPublisherMock.Object);
            var result = new AssociationSearchService(dynamicAssociationsRepositoryFactory, platformMemoryCache, dynamicAssociationsService);

            return result;
        }

        private static Func<IAssociationsRepository> CreateRepositoryMock(IEnumerable<AssociationEntity> entities)
        {
            var dynamicAssociationsRepositoryMock = new Mock<IAssociationsRepository>();
            var entitiesMock = entities.AsQueryable().BuildMock();

            dynamicAssociationsRepositoryMock.Setup(x => x.Associations)
                .Returns(entitiesMock.Object);
            dynamicAssociationsRepositoryMock.Setup(x => x.GetAssociationsByIdsAsync(It.IsAny<string[]>()))
                .Returns<string[]>(ids =>
                    Task.FromResult(entities.Where(x => ids.Contains(x.Id)).ToArray()));

            IAssociationsRepository func() => dynamicAssociationsRepositoryMock.Object;

            return func;
        }

        private static PlatformMemoryCache GetPlatformMemoryCache()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);
            return platformMemoryCache;
        }
    }
}
