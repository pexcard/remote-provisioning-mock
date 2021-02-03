﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using RemoteProvisioningServiceMock.Extensions;
using RemoteProvisioningServiceMock.Storage.Entity;

namespace RemoteProvisioningServiceMock.Storage
{
    public class TokenProvisioningStorage : AzureTableStorageAbstract
    {
        public TokenProvisioningStorage(string connectionString)
            : base(connectionString, "TokenProvisioning") { }

        public async Task Insert(int cardholderId, TokenProvisioningEntity entity, CancellationToken ct)
        {
            entity.PartitionKey = cardholderId.ToString();
            entity.RowKey = $"{Guid.NewGuid():N}";

            var insert = TableOperation.Insert(entity);

            await Table
                .ExecuteAsync(insert, ct);
        }

        public async Task<IList<TokenProvisioningEntity>> GetTokenProvisioning(
            int cardholderId, DateTime from, string search, CancellationToken ct)
        {
            from = ShiftToAllowedIfNeeded(from);

            var query = new TableQuery<TokenProvisioningEntity>()
                .Where(TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, cardholderId.ToString()),
                        TableOperators.And,
                        TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, from)
                    )
                );

            var items = await Table
                .ExecuteQueryAsync(query, ct);

            // no way to do storage table %LIKE% search so we do back-end string contains search
            if (items.Any() && !string.IsNullOrEmpty(search))
            {
                items = items
                    .Where(x => !string.IsNullOrEmpty(x.RequestJson) && x.RequestJson.Contains(search))
                    .ToList();
            }

            return items
                .OrderByDescending(x => x.Timestamp)
                .Take(10) // return top 10
                .ToList();
        }
    }
}