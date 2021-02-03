using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Protocol;

namespace RemoteProvisioningServiceMock.Storage
{
    public abstract class AzureTableStorageAbstract
    {
        protected readonly string ConnectionString;
        protected readonly string StorageTableName;

        protected CloudTable Table;

        protected AzureTableStorageAbstract(string connectionString, string storageTableName)
        {
            ConnectionString = connectionString;
            StorageTableName = storageTableName;
        }

        public async Task<AzureTableStorageAbstract> InitTableAsync(CancellationToken token)
        {
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();

            tableClient.DefaultRequestOptions = new TableRequestOptions
            {
                RetryPolicy = new LinearRetry(TimeSpan.FromMilliseconds(500), 3)
            };

            Table = tableClient.GetTableReference(StorageTableName);

            await Table
                .CreateIfNotExistsAsync(null, null, token);

            return this;
        }

        protected DateTime ShiftToAllowedIfNeeded(DateTime dateTime)
        {
            return dateTime <= TableConstants.MinDateTime 
                ? TableConstants.MinDateTime.DateTime.AddDays(1) 
                : dateTime;
        }
    }
}