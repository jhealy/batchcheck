using System.Text;

namespace DevFish.Azure.Batch.Common.Core
{
    public class AccountSettings
    {
        public string BatchServiceUrl { get; set; }
        public string BatchAccountName { get; set; }
        public string BatchAccountKey { get; set; }

        public string StorageServiceUrl { get; set; }
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            AddSetting(stringBuilder, nameof(BatchAccountName), this.BatchAccountName);
            AddSetting(stringBuilder, nameof(BatchAccountKey), this.BatchAccountKey);
            AddSetting(stringBuilder, nameof(BatchServiceUrl), this.BatchServiceUrl);

            AddSetting(stringBuilder, nameof(StorageAccountName), this.StorageAccountName);
            AddSetting(stringBuilder, nameof(StorageAccountKey), this.StorageAccountKey);
            AddSetting(stringBuilder, nameof(StorageServiceUrl), this.StorageServiceUrl);

            return stringBuilder.ToString();
        }

        private static void AddSetting(StringBuilder stringBuilder, string settingName, object settingValue)
        {
            stringBuilder.AppendFormat("{0} = {1}", settingName, settingValue).AppendLine();
        }
    }
}
