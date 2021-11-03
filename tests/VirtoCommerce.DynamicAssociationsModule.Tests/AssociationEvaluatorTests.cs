using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Models;
using VirtoCommerce.DynamicAssociationsModule.Core.Services;
using VirtoCommerce.DynamicAssociationsModule.Data.Services;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.DynamicAssociationsModule.Tests
{
    public class AssociationEvaluatorTests
    {
        private readonly Mock<IStoreService> _storeServiceMock;
        private readonly Mock<IAssociationConditionSelector> _dynamicAssociationConditionSelectorMock;
        private readonly Mock<IAssociationConditionEvaluator> _dynamicAssociationConditionEvaluatorMock;
        private readonly Mock<IItemService> _itemServiceMock;

        private readonly AssociationEvaluationContext _evaluationContext = new AssociationEvaluationContext();

        public AssociationEvaluatorTests()
        {
            _storeServiceMock = CreateStoreServiceMock();
            _dynamicAssociationConditionSelectorMock = CreateDynamicAssociationConditionSelectorMock();
            _dynamicAssociationConditionEvaluatorMock = CreateDynamicAssociationConditionEvaluatorMock();
            _itemServiceMock = CreateItemServiceMock();
        }

        [Theory]
        [InlineData(null, new string[0])]
        [InlineData(new string[0], new string[0])]
        public async Task EvaluateDynamicAssociations_ProductsToMatchEmpty_EmptyCollectionResult(string[] productsToMatch, string[] expectedResult)
        {
            // Arrange
            var evaluator = CreateDynamicAssociationEvaluator();
            _evaluationContext.ProductsToMatch = productsToMatch;

            // Act
            var result = await evaluator.EvaluateAssociationsAsync(_evaluationContext);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task EvaluateDynamicAssociations_DynamicRuleNotFound_EmptyCollectionResult()
        {
            // Arrange
            _evaluationContext.ProductsToMatch = new[] { string.Empty, };

            _storeServiceMock
                .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Store());

            _itemServiceMock
                .Setup(x => x.GetByIdsAsync(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[]
                {
                    new CatalogProduct(),
                });

            var evaluator = CreateDynamicAssociationEvaluator();

            // Act
            var result = await evaluator.EvaluateAssociationsAsync(_evaluationContext);

            // Assert
            _dynamicAssociationConditionEvaluatorMock
                .Verify(x => x.EvaluateAssociationConditionAsync(It.IsAny<AssociationConditionEvaluationRequest>()), Times.Never);

            Assert.Empty(result);
        }

        [Fact]
        public async Task EvaluateDynamicAssociations()
        {
            // Arrange
            _evaluationContext.ProductsToMatch = new[] { string.Empty, };

            _storeServiceMock
                .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Store());

            _itemServiceMock
                .Setup(x => x.GetByIdsAsync(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[]
                {
                    new CatalogProduct(),
                });

            _dynamicAssociationConditionSelectorMock
                .Setup(x => x.GetAssociationConditionAsync(It.IsAny<AssociationEvaluationContext>(), It.IsAny<CatalogProduct>()))
                .ReturnsAsync(new AssociationConditionEvaluationRequest());

            _dynamicAssociationConditionEvaluatorMock
                .Setup(x => x.EvaluateAssociationConditionAsync(It.IsAny<AssociationConditionEvaluationRequest>()))
                .ReturnsAsync(new AssociationConditionEvaluationResult());

            var evaluator = CreateDynamicAssociationEvaluator();

            // Act
            var result = await evaluator.EvaluateAssociationsAsync(_evaluationContext);

            // Assert
            Assert.Empty(result);

            _dynamicAssociationConditionEvaluatorMock
                .Verify(
                    x => x.EvaluateAssociationConditionAsync(It.IsAny<AssociationConditionEvaluationRequest>()),
                    Times.AtLeastOnce
                    );
        }


        private static Mock<IStoreService> CreateStoreServiceMock()
        {
            var result = new Mock<IStoreService>();

            return result;
        }

        private static Mock<IAssociationConditionSelector> CreateDynamicAssociationConditionSelectorMock()
        {
            var result = new Mock<IAssociationConditionSelector>();

            return result;
        }

        private static Mock<IItemService> CreateItemServiceMock()
        {
            var result = new Mock<IItemService>();

            return result;
        }

        private static Mock<IAssociationConditionEvaluator> CreateDynamicAssociationConditionEvaluatorMock()
        {
            var result = new Mock<IAssociationConditionEvaluator>();

            return result;
        }

        private IAssociationEvaluator CreateDynamicAssociationEvaluator()
        {
            var result = new AssociationEvaluator(
                _storeServiceMock.Object,
                _dynamicAssociationConditionSelectorMock.Object,
                _itemServiceMock.Object,
                _dynamicAssociationConditionEvaluatorMock.Object
                );

            return result;
        }
    }
}
