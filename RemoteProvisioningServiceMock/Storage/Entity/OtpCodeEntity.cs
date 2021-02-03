using Microsoft.Azure.Cosmos.Table;

namespace RemoteProvisioningServiceMock.Storage.Entity
{
    public class OtpCodeEntity : TableEntity
    {
        public string SharedSecret { get; set; }

        public int StatusCode { get; set; }

        public int DelayMls { get; set; }

        public string RequestJson { get; set; }
    }
}