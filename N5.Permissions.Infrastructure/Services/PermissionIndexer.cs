using N5.Permissions.Application.Common.Interfaces;
using N5.Permissions.Application.Common.Models;
using N5.Permissions.Domain.Entities;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Infrastructure.Services
{
    public class PermissionIndexer : IPermissionIndexer
    {
        private readonly IElasticClient _elasticClient;
        private const string IndexName = "permissions";
        public PermissionIndexer(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }
        public async Task IndexAsync(PermissionDocument document, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si el índice existe, si no, crearlo
                var indexExists = await _elasticClient.Indices.ExistsAsync(IndexName, ct: cancellationToken);
                if (!indexExists.Exists)
                {
                    var createIndexResponse = await _elasticClient.Indices.CreateAsync(IndexName, c => c
                        .Map<PermissionDocument>(m => m
                            .AutoMap()
                        ), cancellationToken);

                    if (!createIndexResponse.IsValid)
                    {
                        throw new Exception($"Failed to create index: {createIndexResponse.DebugInformation}");
                    }
                }

                var uniqueId = $"{document.Id}-{document.Operation.ToLower()}";

                var response = await _elasticClient.IndexAsync(document, idx => idx
                    .Index(IndexName)
                    .Id(uniqueId)
                    .Refresh(Elasticsearch.Net.Refresh.WaitFor), cancellationToken);

                if (!response.IsValid)
                {
                    throw new Exception($"Failed to index document: {response.DebugInformation}");
                }

                var searchResponse = await _elasticClient.SearchAsync<PermissionDocument>(s => s
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.Id)
                            .Value(document.Id)
                        )
                    ));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
