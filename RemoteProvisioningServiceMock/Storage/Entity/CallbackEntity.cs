using Microsoft.Azure.Cosmos.Table;

namespace RemoteProvisioningServiceMock.Storage.Entity
{
    public class CallbackEntity : TableEntity
    {
        public string SharedSecret { get; set; }

        public string RequestJson { get; set; }
    }
}