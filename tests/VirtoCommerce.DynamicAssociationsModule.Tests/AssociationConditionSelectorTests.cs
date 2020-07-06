using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Conditions;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Search;
using VirtoCommerce.DynamicAssociationsModule.Core.Search;
using VirtoCommerce.DynamicAssociationsModule.Core.Services;
using VirtoCommerce.DynamicAssociationsModule.Data.Services;
using Xunit;

namespace VirtoCommerce.DynamicAssociationsModule.Tests
{
    public class AssociationConditionSelectorTests
    {
        private readonly Mock<IAssociationSearchService> _dynamicAssociationSearchServiceMock;

        private readonly AssociationEvaluationContext _evaluationContext = new AssociationEvaluationContext();
        private readonly CatalogProduct _catalogProduct = new CatalogProduct();

        public AssociationConditionSelectorTests()
        {
            _dynamicAssociationSearchServiceMock = CreateDynamicAssociationSearchServiceMock();
        }

        [Fact]
        public async Task GetDynamicAssociationCondition_MatchingRule_ExceptionThrown()
        {
            // Arrange
            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchAssociationsAsync(It.IsAny<AssociationSearchCriteria>()))
                .ReturnsAsync(new AssociationSearchResult
                {
                    Results = new List<Association>
                    {
                        new Association
                        {
                            ExpressionTree = new AssociationRuleTree
                            {
                                Children = new []
                                {
                                    new Mock<IConditionTree>().Object
                                },
                            },
                        },
                    },
                });

            var selector = CreateDynamicAssociationConditionSelector();

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await selector.GetAssociationConditionAsync(_evaluationContext, _catalogProduct)
                );
        }

        [Fact]
        public async Task GetDynamicAssociationCondition_ResultRule_ExceptionThrown()
        {
            // Arrange
            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchAssociationsAsync(It.IsAny<AssociationSearchCriteria>()))
                .ReturnsAsync(new AssociationSearchResult
                {
                    Results = new List<Association> {
                        new Association
                        {
                            ExpressionTree = new AssociationRuleTree
                            {
                                Children = new IConditionTree[]
                                {
                                    CreateBlockMatchingRulesMock().Object,
                                },
                            },
                        },
                    },
                });

            var selector = CreateDynamicAssociationConditionSelector();

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await selector.GetAssociationConditionAsync(_evaluationContext, _catalogProduct)
                );
        }

        [Fact]
        public async Task GetDynamicAssociationCondition_OutputTuningBlock_ExceptionThrown()
        {
            // Arrange
            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchAssociationsAsync(It.IsAny<AssociationSearchCriteria>()))
                .ReturnsAsync(new AssociationSearchResult
                {
                    Results = new List<Association>
                    {
                        new Association
                        {
                            ExpressionTree = new AssociationRuleTree
                            {
                                Children = new IConditionTree[]
                                {
                                    CreateBlockMatchingRulesMock().Object,
                                    CreateBlockResultingRules()
                                },
                            },
                        },
                    },
                });

            var selector = CreateDynamicAssociationConditionSelector();
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await selector.GetAssociationConditionAsync(_evaluationContext, _catalogProduct)
                );
        }

        [Fact]
        public async Task GetDynamicAssociationCondition_SearchResultIsEmpty_NullReturned()
        {
            // Arrange
            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchAssociationsAsync(It.IsAny<AssociationSearchCriteria>()))
                .ReturnsAsync(new AssociationSearchResult { Results = new List<Association>(), });

            var selector = CreateDynamicAssociationConditionSelector();

            // Act
            var result = await selector.GetAssociationConditionAsync(_evaluationContext, _catalogProduct);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DynamicAssociationRule_Search_Succesed()
        {
            // Arrange
            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchAssociationsAsync(It.IsAny<AssociationSearchCriteria>()))
                .ReturnsAsync(new AssociationSearchResult
                {
                    Results = new List<Association>
                    {
                        new Association
                        {
                            ExpressionTree = new AssociationRuleTree
                            {
                                Children = new IConditionTree[]
                                {
                                    CreateBlockMatchingRulesMock().Object,
                                    CreateBlockResultingRules(),
                                    new BlockOutputTuning(),
                                },
                            },
                        },
                    },
                });

            var selector = CreateDynamicAssociationConditionSelector();

            // Act
            var result = await selector.GetAssociationConditionAsync(_evaluationContext, _catalogProduct);

            // Assert
            Assert.IsType<AssociationConditionEvaluationRequest>(result);
        }

        [Fact]
        public async Task GetDynamicAssociationCondition_MultipleResultFetched_FindSingleRule()
        {
            // Arrange
            var matchingRuleMock = CreateBlockMatchingRulesMock();

            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchAssociationsAsync(It.IsAny<AssociationSearchCriteria>()))
                .ReturnsAsync(new AssociationSearchResult
                {
                    Results = new[]
                    {
                        new Association
                        {
                            ExpressionTree = new AssociationRuleTree
                            {
                                Children = new IConditionTree[]
                                {
                                    CreateBlockMatchingRulesMock().Object,
                                    CreateBlockResultingRules(),
                                    new BlockOutputTuning(),
                                }
                            },
                        },
                        new Association
                        {
                            ExpressionTree = new AssociationRuleTree
                            {
                                Children = new IConditionTree[]
                                {
                                    matchingRuleMock.Object,
                                },
                            },
                        },
                    },
                });

            var selector = CreateDynamicAssociationConditionSelector();

            // Act
            var result = await selector.GetAssociationConditionAsync(_evaluationContext, _catalogProduct);

            // Assert
            Assert.IsType<AssociationConditionEvaluationRequest>(result);
            matchingRuleMock.Verify(x => x.IsSatisfiedBy(It.IsAny<IEvaluationContext>()), Times.Never);
        }


        [Theory]
        [InlineData(100, 101, 10, 0)]
        [InlineData(1000, 980, 30, 20)]
        [InlineData(1000, 0, 1000, 1000)]
        public async Task GetDynamicAssociationCondition_Pagination_Succeded(int limit, int skip, int take, int expecting)
        {
            // Arrange
            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchAssociationsAsync(It.IsAny<AssociationSearchCriteria>()))
                .ReturnsAsync(new AssociationSearchResult
                {
                    Results = new[]
                    {
                        new Association
                        {
                            ExpressionTree = new AssociationRuleTree
                            {
                                Children = new IConditionTree[]
                                {
                                    CreateBlockMatchingRulesMock().Object,
                                    CreateBlockResultingRules(),
                                    new BlockOutputTuning
                                    {
                                        OutputLimit = limit
                                    },
                                },
                            },
                        },
                    },
                });

            _evaluationContext.Skip = skip;
            _evaluationContext.Take = take;

            var selector = CreateDynamicAssociationConditionSelector();

            // Act
            var result = await selector.GetAssociationConditionAsync(_evaluationContext, _catalogProduct);

            // Assert
            Assert.Equal(expecting, result.Take);
        }


        private static Mock<IAssociationSearchService> CreateDynamicAssociationSearchServiceMock()
        {
            var result = new Mock<IAssociationSearchService>();

            return result;
        }

        private IAssociationConditionSelector CreateDynamicAssociationConditionSelector()
        {
            var result = new AssociationConditionsSelector(_dynamicAssociationSearchServiceMock.Object);

            return result;
        }

        private static Mock<BlockMatchingRules> CreateBlockMatchingRulesMock()
        {
            var result = new Mock<BlockMatchingRules>();
            result
                .Setup(x => x.IsSatisfiedBy(It.IsAny<IEvaluationContext>()))
                .Returns(true);

            return result;
        }

        private static BlockResultingRules CreateBlockResultingRules()
        {
            var result = new BlockResultingRules
            {
                Children = new IConditionTree[]
                {
                    new ConditionPropertyValues
                    {
                        Properties = new[]
                        {
                            new Property
                            {
                                Name = string.Empty,
                                Values = new []
                                {
                                    new PropertyValue { Value = string.Empty, }
                                }
                            }
                        }
                    },
                    new ConditionProductCategory { CategoryIds = Array.Empty<string>(), }
                }
            };

            return result;
        }
    }
}
