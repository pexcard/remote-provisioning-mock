using System.Collections.Generic;

namespace RemoteProvisioningServiceMock.Models
{
    public class DecisionResponse
    {
        public int AcctId { get; set; }

        public int BusinessAcctId { get; set; }

        public string Last4CardNumber { get; set; }

        public List<ContactItem> Contacts { get; set; }
    }
}