using System;

namespace RemoteProvisioningServiceMock.Models
{
    public class CallbackRequest
    {
        public DateTime CallbackTime { get; set; }

        public CallbackData Data { get; set; }
    }
}