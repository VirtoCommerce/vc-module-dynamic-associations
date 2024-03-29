using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Search;
using VirtoCommerce.DynamicAssociationsModule.Web.Authorization;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.DynamicAssociationsModule.Tests
{
    public class AssociationAuthorizationHandlerTests
    {
        public static object[][] EmptyCategories = new object[][]
        {
            new object[] { null },
            new object[] { Array.Empty<string>() }
        };

        private readonly IOptions<MvcNewtonsoftJsonOptions> _jsonOptions;
        private const string _permission = "test:permission";

        public AssociationAuthorizationHandlerTests()
        {
            AbstractTypeFactory<PermissionScope>.RegisterType<SelectedCatalogScope>();
            _jsonOptions = CreateJsonConverterWrapper();
            _jsonOptions.Value.SerializerSettings.Converters.Add(new PolymorphJsonConverter());
        }

        [Fact]
        public async Task Handle_Permission_With_Correct_Scope_Catalog_Succeded()
        {
            // Arrange
            var storeMock = CreateStoreServiceMock();

            storeMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[] { new Store
                {
                    Id = "testStore",
                    Catalog = "testCatalog"
                }});

            var authorizationHandler = CreateAuthorizationHandler(storeMock.Object, null);

            var dynamicAssociation = new Association { StoreId = "testStore" };

            var context = CreateAuthorizationHandlerContext(
                _permission,
                $"{_permission}|[{{\"catalogId\":\"testCatalog\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog\"}}]",
                dynamicAssociation);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task Handle_Permission_With_Incorrect_Scope_Catalog_Unsecceded()
        {
            // Arrange
            var storeMock = CreateStoreServiceMock();

            storeMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[] { new Store
                {
                    Id = "testStore",
                    Catalog = "testCatalog1"
                }});

            var authorizationHandler = CreateAuthorizationHandler(storeMock.Object, null);

            var dynamicAssociation = new Association { StoreId = "testStore" };

            var context = CreateAuthorizationHandlerContext(
                _permission,
                $"{_permission}|[{{\"catalogId\":\"testCatalog\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog\"}}]",
                dynamicAssociation);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task Handle_Permission_Without_Scope_Succeded()
        {
            // Arrange
            var storeMock = CreateStoreServiceMock();

            storeMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[] { new Store
                {
                    Id = "testStore",
                    Catalog = "testCatalog"
                }});

            var authorizationHandler = CreateAuthorizationHandler(storeMock.Object, null);

            var dynamicAssociation = new Association();

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}",
                dynamicAssociation);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task Handle_DynamicAssociationSearchCriteria_With_Allowed_Catalog_Succeded()
        {
            // Arrange
            var storeMock = CreateStoreServiceMock();

            storeMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[]
                {
                    new Store
                    {
                        Catalog = "testCatalog",
                        Id = "testStore1",
                    },
                    new Store
                    {
                        Catalog = "testCatalog2",
                        Id = "testStore2",
                    },
                });

            var authorizationHandler = CreateAuthorizationHandler(storeMock.Object, null);
            var dynamicAssociationSearchCriteria = new AssociationSearchCriteria
            {
                StoreIds = Array.Empty<string>(),
            };

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}|[{{\"catalogId\":\"testCatalog\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog\"}}]",
                dynamicAssociationSearchCriteria);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.Equal(new[] { "testStore1" }, dynamicAssociationSearchCriteria.StoreIds);
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task Handle_DynamicAssociationSearchCriteria_With_Null_StoreIds_Succeded()
        {
            // Arrange
            var storeMock = CreateStoreServiceMock();

            storeMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(Array.Empty<Store>());

            var authorizationHandler = CreateAuthorizationHandler(storeMock.Object, null);
            var dynamicAssociationSearchCriteria = new AssociationSearchCriteria();

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}|[{{\"catalogId\":\"testCatalog\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog\"}}]",
                dynamicAssociationSearchCriteria);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.Equal(Array.Empty<string>(), dynamicAssociationSearchCriteria.StoreIds);
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task Handle_DynamicAssociationSearchCriteria_Without_Scope_Succeded()
        {
            // Arrange
            var storeMock = CreateStoreServiceMock();

            storeMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(Array.Empty<Store>());

            var authorizationHandler = CreateAuthorizationHandler(storeMock.Object, null);
            var dynamicAssociationSearchCriteria = new AssociationSearchCriteria
            {
                StoreIds = new[] { "testStore1", "testStore2" },
            };

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}",
                dynamicAssociationSearchCriteria);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.Equal(new[] { "testStore1", "testStore2" }, dynamicAssociationSearchCriteria.StoreIds);
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task Handle_DynamicAssociationCollection_Without_Scope_Succeded()
        {
            // Arrange
            var storeMock = CreateStoreServiceMock();

            var authorizationHandler = CreateAuthorizationHandler(storeMock.Object, null);
            var dynamicAssociations = Array.Empty<Association>();

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}",
                dynamicAssociations);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task Handle_DynamicAssociationCollection_Incorrect_With_Scope_Unsucceded()
        {
            // Arrange
            var storeMock = CreateStoreServiceMock();

            storeMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[]
                {
                    new Store
                    {
                        Catalog = "testCatalog1",
                    },
                    new Store
                    {
                        Catalog = "testCatalog2",
                    },
                });

            var authorizationHandler = CreateAuthorizationHandler(storeMock.Object, null);
            var dynamicAssociations = Array.Empty<Association>();

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}|[{{\"catalogId\":\"testCatalog1\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog1\"}}]",
                dynamicAssociations);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task Handle_DynamicAssociationCollection_Correct_With_Scope_Succeded()
        {
            // Arrange
            var storeMock = CreateStoreServiceMock();

            storeMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[]
                {
                    new Store
                    {
                        Catalog = "testCatalog1",
                    },
                    new Store
                    {
                        Catalog = "testCatalog2",
                    },
                });

            var authorizationHandler = CreateAuthorizationHandler(storeMock.Object, null);
            var dynamicAssociations = Array.Empty<Association>();

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}|[" +
                "{\"catalogId\":\"testCatalog1\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog1\"}," +
                "{\"catalogId\":\"testCatalog2\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog2\"}" +
                "]",
                dynamicAssociations);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task Handle_ProductsToMatch_Correct_With_Scope_Succeded()
        {
            // Arrange
            var storeMock = CreateStoreServiceMock();
            storeMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[] { new Store { Catalog = "testCatalog1" } });

            var authorizationHandler = CreateAuthorizationHandler(storeMock.Object, null);
            var productsToMatchCriteria = new AssociationEvaluationContext { StoreId = "testStore" };

            var context = CreateAuthorizationHandlerContext(
                _permission,
                $"{_permission}|[{{\"catalogId\":\"testCatalog1\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog1\"}}]",
                productsToMatchCriteria);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task Handle_ProductsToMatch_Incorrect_With_Scope_UnSucceded()
        {
            // Arrange
            var storeMock = CreateStoreServiceMock();
            storeMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[] { new Store { Catalog = "testCatalog" } });

            var authorizationHandler = CreateAuthorizationHandler(storeMock.Object, null);
            var productsToMatchCriteria = new AssociationEvaluationContext { StoreId = "testStore" };

            var context = CreateAuthorizationHandlerContext(
                _permission,
                $"{_permission}|[{{\"catalogId\":\"testCatalog1\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog1\"}}]",
                productsToMatchCriteria);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task Handle_ConditionEvaluationRequest_Category_Succeded()
        {
            // Arrage
            var categoryServiceMock = CreateCategoryServiceMock();
            categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[]
                {
                    new Category
                    {
                        Id = "testCategory1",
                        Outlines = new[]
                        {
                            new Outline
                            {
                                Items = new []
                                {
                                    new OutlineItem
                                    {
                                        Id = "testCatalog1",
                                    },
                                }
                            },
                        }
                    },
                });

            var authorizationHandler = CreateAuthorizationHandler(null, categoryServiceMock.Object);
            var evaluationRequest = new AssociationConditionEvaluationRequest
            {
                CategoryIds = new[] { "testCategory1" }
            };

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}|[{{\"catalogId\":\"testCatalog1\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog1\"}}]",
                evaluationRequest);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task Handle_ConditionEvaluationRequest_Category_Unsucceded()
        {
            // Arrage
            var categoryServiceMock = CreateCategoryServiceMock();
            categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[]
                {
                    new Category
                    {
                        Id = "testCategory1",
                        Outlines = new[]
                        {
                            new Outline
                            {
                                Items = new []
                                {
                                    new OutlineItem
                                    {
                                        Id = "testCatalog2",
                                    },
                                }
                            },
                        }
                    },
                });

            var authorizationHandler = CreateAuthorizationHandler(null, categoryServiceMock.Object);
            var evaluationRequest = new AssociationConditionEvaluationRequest
            {
                CategoryIds = new[] { "testCategory1" }
            };

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}|[{{\"catalogId\":\"testCatalog1\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog1\"}}]",
                evaluationRequest);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_ConditionEvaluationRequest_CategoryDoesNotExists_Succeeded()
        {
            // Arrage
            var categoryServiceMock = CreateCategoryServiceMock();
            categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(Array.Empty<Category>);

            var authorizationHandler = CreateAuthorizationHandler(null, categoryServiceMock.Object);
            var evaluationRequest = new AssociationConditionEvaluationRequest
            {
                CategoryIds = new[] { "testCategory1" }
            };

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}|[{{\"catalogId\":\"testCatalog1\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog1\"}}]",
                evaluationRequest);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Theory]
        [MemberData(nameof(EmptyCategories))]
        public async Task HandleRequirementAsync_ConditionEvaluationRequest_EmptyCategoryList_Fails(string[] cetegoryIds)
        {
            // Arrage
            var categoryServiceMock = CreateCategoryServiceMock();
            categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[]
                {
                    new Category
                    {
                        Id = "testCategory1",
                        Outlines = new[]
                        {
                            new Outline
                            {
                                Items = new []
                                {
                                    new OutlineItem
                                    {
                                        Id = "testCatalog1",
                                    },
                                }
                            },
                        }
                    },
                });

            var authorizationHandler = CreateAuthorizationHandler(null, categoryServiceMock.Object);
            var evaluationRequest = new AssociationConditionEvaluationRequest
            {
                CategoryIds = cetegoryIds,
            };

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}|[{{\"catalogId\":\"testCatalog1\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog1\"}}]",
                evaluationRequest);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_ConditionEvaluationRequest_NoCatalogSpecifiedAndValidCategories_Succeeded()
        {
            // Arrage
            var categoryServiceMock = CreateCategoryServiceMock();
            categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[]
                {
                    new Category
                    {
                        Id = "testCategory1",
                        Outlines = new[]
                        {
                            new Outline
                            {
                                Items = new []
                                {
                                    new OutlineItem
                                    {
                                        Id = "testCatalog1",
                                    },
                                }
                            },
                        }
                    },
                });

            var authorizationHandler = CreateAuthorizationHandler(null, categoryServiceMock.Object);
            var evaluationRequest = new AssociationConditionEvaluationRequest
            {
                CatalogId = null,
                CategoryIds = new[] { "testCategory1" }
            };

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}|[{{\"catalogId\":\"testCatalog1\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog1\"}}]",
                evaluationRequest);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_ConditionEvaluationRequest_ValidCatalogAndValidCategories_Succeeded()
        {
            // Arrage
            var categoryServiceMock = CreateCategoryServiceMock();
            categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[]
                {
                    new Category
                    {
                        Id = "testCategory1",
                        Outlines = new[]
                        {
                            new Outline
                            {
                                Items = new []
                                {
                                    new OutlineItem
                                    {
                                        Id = "testCatalog1",
                                    },
                                }
                            },
                        }
                    },
                });

            var authorizationHandler = CreateAuthorizationHandler(null, categoryServiceMock.Object);
            var evaluationRequest = new AssociationConditionEvaluationRequest
            {
                CatalogId = "testCatalog1",
                CategoryIds = new[] { "testCategory1" }
            };

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}|[{{\"catalogId\":\"testCatalog1\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog1\"}}]",
                evaluationRequest);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_ConditionEvaluationRequest_NotValidCatalogAndValidCategories_Fails()
        {
            // Arrage
            var categoryServiceMock = CreateCategoryServiceMock();
            categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[]
                {
                    new Category
                    {
                        Id = "testCategory1",
                        Outlines = new[]
                        {
                            new Outline
                            {
                                Items = new []
                                {
                                    new OutlineItem
                                    {
                                        Id = "testCatalog1",
                                    },
                                }
                            },
                        }
                    },
                });

            var authorizationHandler = CreateAuthorizationHandler(null, categoryServiceMock.Object);
            var evaluationRequest = new AssociationConditionEvaluationRequest
            {
                CatalogId = "testCatalog2",
                CategoryIds = new[] { "testCategory1" }
            };

            var context = CreateAuthorizationHandlerContext(
                $"{_permission}",
                $"{_permission}|[{{\"catalogId\":\"testCatalog1\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog1\"}}]",
                evaluationRequest);

            // Act
            await authorizationHandler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }


        private IOptions<MvcNewtonsoftJsonOptions> CreateJsonConverterWrapper()
        {
            return Options.Create(new MvcNewtonsoftJsonOptions());
        }

        private static ICollection<IAuthorizationRequirement> CreateAuthorizationRequirements(string permission)
        {
            var result = new List<IAuthorizationRequirement>
            {
                new AssociationAuthorizationRequirement(permission)
            };

            return result;
        }

        private static ClaimsPrincipal CreateClaimsPtincipal(string permissionValue)
        {
            var claim = new Claim("permission", permissionValue);
            var claimsIdentity = new ClaimsIdentity(new[] { claim });
            var result = new ClaimsPrincipal(claimsIdentity);

            return result;
        }

        private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(string permission, string permissionValue, object resource)
        {
            var requirements = CreateAuthorizationRequirements(permission);
            var claimsPrincipal = CreateClaimsPtincipal(permissionValue);

            var result = new AuthorizationHandlerContext(requirements, claimsPrincipal, resource);

            return result;
        }

        private static Mock<IStoreService> CreateStoreServiceMock()
        {
            var result = new Mock<IStoreService>();

            return result;
        }

        private static Mock<ICategoryService> CreateCategoryServiceMock()
        {
            var result = new Mock<ICategoryService>();

            return result;
        }

        private IAuthorizationHandler CreateAuthorizationHandler(IStoreService storeService, ICategoryService categoryService)
        {
            var result = new AssociationAuthorizationHandler(_jsonOptions, storeService, categoryService);

            return result;
        }
    }

    //TODO: Remove after adding this converters to VirtoCommerce.Platform.Core package
    public class PolymorphJsonConverter : JsonConverter
    {
        private static readonly Type[] _knowTypes = { typeof(ObjectSettingEntry), typeof(DynamicProperty), typeof(ApplicationUser), typeof(Role), typeof(PermissionScope) };

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return _knowTypes.Any(x => x.IsAssignableFrom(objectType));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object result;
            var obj = JObject.Load(reader);
            if (typeof(PermissionScope).IsAssignableFrom(objectType))
            {
                var scopeType = objectType.Name;
                var pt = obj["type"] ?? obj["Type"];
                if (pt != null)
                {
                    scopeType = pt.Value<string>();
                }
                result = AbstractTypeFactory<PermissionScope>.TryCreateInstance(scopeType);
                if (result == null)
                {
                    throw new NotSupportedException("Unknown scopeType: " + scopeType);
                }
            }
            else
            {
                var tryCreateInstance = typeof(AbstractTypeFactory<>).MakeGenericType(objectType).GetMethods().FirstOrDefault(x => x.Name.EqualsInvariant("TryCreateInstance") && x.GetParameters().Length == 0);
                result = tryCreateInstance?.Invoke(null, null);
            }

            serializer.Populate(obj.CreateReader(), result);
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
