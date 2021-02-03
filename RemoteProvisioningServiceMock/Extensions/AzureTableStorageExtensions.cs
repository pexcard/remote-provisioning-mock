using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using RemoteProvisioningServiceMock.Storage;

namespace RemoteProvisioningServiceMock.Extensions
{
    public static class AzureTableStorageExtensions
    {
        public static TProvider InitTable<TProvider>(this TProvider provider, CancellationToken token = default)
            where TProvider : AzureTableStorageAbstract
        {
            return (TProvider)provider
                .InitTableAsync(token)
                .GetAwaiter()
                .GetResult();
        }

        public static async Task<IList<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query, CancellationToken ct = default)
            where T : ITableEntity, new()
        {
            var items = new List<T>();

            TableContinuationToken token = null;

            do
            {
                var tableQuerySegment = await table
                    .ExecuteQuerySegmentedAsync(query, token, ct);

                token = tableQuerySegment.ContinuationToken;

                items.AddRange(tableQuerySegment);

            } while (token != null && !ct.IsCancellationRequested);

            return items;
        }
    }
}