using System.Collections.Generic;

namespace RemoteProvisioningServiceMock.Models
{
    public class ResponseSettingsModel
    {
        public string SharedSecret { get; set; }

        public int StatusCode { get; set; }

        public int TimeoutMls { get; set; }

        public List<ContactItem> Contacts { get; set; }
    }
}