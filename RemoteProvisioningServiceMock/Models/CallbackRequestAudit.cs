namespace RemoteProvisioningServiceMock.Models
{
    public class CallbackRequestAudit
    {
        public string SharedSecret { get; set; }

        public CallbackRequest Request { get; set; }
    }
}