namespace RemoteProvisioningServiceMock.Models
{
    public class SendOtpRequestAudit
    {
        public string SharedSecret { get; set; }

        public int StatusCode { get; set; }

        public int DelayMls { get; set; }

        public SendOtpRequest Request { get; set; }
    }
}