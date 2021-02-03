using System;

namespace RemoteProvisioningServiceMock.Models
{
    public class CallbackData
    {
        public string ReferenceId { get; set; }

        public int BusinessAccountId { get; set; }

        public int AcctId { get; set; }

        public int CardId { get; set; }

        public string Last4CardNumber { get; set; }

        public string Last4TokenNumber { get; set; }

        public DateTime? TokenExpirationDate { get; set; }

        public string DeviceName { get; set; }

        public string WalletName { get; set; }

        public string RequestorId { get; set; }

        public bool IsDeviceWallet { get; set; }
    }
}