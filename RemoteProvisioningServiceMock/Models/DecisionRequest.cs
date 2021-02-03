namespace RemoteProvisioningServiceMock.Models
{
    public class DecisionRequest
    {
        public int AcctId { get; set; }

        public int BusinessAcctId { get; set; }

        public string ReferenceId { get; set; }

        public string Last4CardNumber { get; set; }

        public DeviceInfo DeviceInfo { get; set; }
    }
}