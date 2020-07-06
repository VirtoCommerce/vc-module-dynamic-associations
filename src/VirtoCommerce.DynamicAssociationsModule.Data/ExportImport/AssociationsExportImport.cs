using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Search;
using VirtoCommerce.DynamicAssociationsModule.Core.Search;
using VirtoCommerce.DynamicAssociationsModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Data.ExportImport;

namespace VirtoCommerce.DynamicAssociationsModule.Data.ExportImport
{
    public class AssociationsExportImport
    {
        private readonly IAssociationService _associationService;
        private readonly IAssociationSearchService _associationSearchService;
        private readonly JsonSerializer _serializer;
        private readonly int _batchSize = 50;

        public AssociationsExportImport(IAssociationService associationService, IAssociationSearchService associationSearchService, JsonSerializer jsonSerializer)
        {
            _associationService = associationService;
            _associationSearchService = associationSearchService;
            _serializer = jsonSerializer;
        }

        public async Task DoExportAsync(Stream outStream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var progressInfo = new ExportImportProgressInfo { Description = "Loading data..." };
            progressCallback(progressInfo);

            using (var sw = new StreamWriter(outStream, Encoding.UTF8))
            using (var writer = new JsonTextWriter(sw))
            {
                await writer.WriteStartObjectAsync();
                await writer.WritePropertyNameAsync("DynamicAssociations");

                await writer.SerializeJsonArrayWithPagingAsync(_serializer, _batchSize, async (skip, take) =>
                        (GenericSearchResult<Association>)await _associationSearchService.SearchAssociationsAsync(new AssociationSearchCriteria { Skip = skip, Take = take })
                    , (processedCount, totalCount) =>
                    {
                        progressInfo.Description = $"{ processedCount } of { totalCount } dynamic associations have been exported";
                        progressCallback(progressInfo);
                    }, cancellationToken);

                await writer.WriteEndObjectAsync();
                await writer.FlushAsync();
            }
        }

        public async Task DoImportAsync(Stream inputStream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var progressInfo = new ExportImportProgressInfo();
            progressInfo.Description = "Importing dynamic associations…";
            progressCallback(progressInfo);

            using (var streamReader = new StreamReader(inputStream))
            using (var reader = new JsonTextReader(streamReader))
            {
                while (reader.Read())
                {
                    if (reader.TokenType != JsonToken.PropertyName) continue;
                    if (reader.Value.ToString() == "DynamicAssociations")
                    {
                        await reader.DeserializeJsonArrayWithPagingAsync<Association>(_serializer, _batchSize, async items =>
                        {
                            await _associationService.SaveChangesAsync(items.ToArray());
                        }, processedCount =>
                        {
                            progressInfo.Description = $"{processedCount} Tagged items have been imported";
                            progressCallback(progressInfo);
                        }, cancellationToken);
                    }
                }
            }
        }
    }
}
