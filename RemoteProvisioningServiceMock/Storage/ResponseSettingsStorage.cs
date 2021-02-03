using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using RemoteProvisioningServiceMock.Storage.Entity;
using RemoteProvisioningServiceMock.Extensions;

namespace RemoteProvisioningServiceMock.Storage
{
    public class ResponseSettingsStorage : AzureTableStorageAbstract
    {
        public ResponseSettingsStorage(string connectionString)
            : base(connectionString, "ResponseSettings") { }

        public async Task<ResponseSettingsEntity> Get(int businessId, int cardholderId, CancellationToken ct)
        {
            var query = new TableQuery<ResponseSettingsEntity>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, businessId.ToString()), 
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, cardholderId.ToString()))
                );

            var items = await Table
                .ExecuteQueryAsync(query, ct);

            return items.FirstOrDefault();
        }

        public async Task InsertOrMerge(int businessId, int cardholderId, ResponseSettingsEntity entity, CancellationToken ct)
        {
            entity.PartitionKey = businessId.ToString();
            entity.RowKey = cardholderId.ToString();

            var insertOrMerge = TableOperation.InsertOrMerge(entity);

            await Table
                .ExecuteAsync(insertOrMerge, ct);
        }
    }
}