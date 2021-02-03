using System;

namespace RemoteProvisioningServiceMock.Models
{
    public class SendOtpRequest
    {
        public int AcctId { get; set; }

        public int BusinessAcctId { get; set; }

        public string Last4CardNumber { get; set; }

        // this id was provided by the customer's provisioning endpoint
        public string ContactId { get; set; }

        public string PassCode { get; set; }

        public DateTime? Expiration { get; set; }
    }
}