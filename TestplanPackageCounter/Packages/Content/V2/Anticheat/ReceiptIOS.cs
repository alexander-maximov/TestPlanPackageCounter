using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestplanPackageCounter.General;

namespace TestplanPackageCounter.Packages.Content.V2.Anticheat
{
    public class ReceiptIOS : ReceiptContainer
    {
        [JsonProperty("receipt-data")]
        [DefaultValue(Constants.NotExistsString)]
        public string ReceiptData { get; private set; }

        [JsonProperty("password")]
        [DefaultValue(Constants.NotExistsString)]
        public string Password { get; private set; }

        [JsonProperty("exclude-old-transactions")]
        [DefaultValue(null)]
        public bool? ExcludeOldTransactions { get; private set; }
    }
}
