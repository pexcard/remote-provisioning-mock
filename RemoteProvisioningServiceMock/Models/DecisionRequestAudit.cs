namespace RemoteProvisioningServiceMock.Models
{
    public class DecisionRequestAudit
    {
        public string SharedSecret { get; set; }

        public int StatusCode { get; set; }

        public int DelayMls { get; set; }

        public DecisionRequest Request { get; set; }

        public DecisionResponse Response { get; set; }
    }
}