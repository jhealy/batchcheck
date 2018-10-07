using System.Text;

namespace DevFish.Azure.Batch.Common.Core
{
    public class JobSettings
    {
        public string PoolID { get; set; }
        public string JobID { get; set; }
        public string BatchAccountKey { get; set; }

        public string StorageServiceUrl { get; set; }
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            AddSetting(stringBuilder, nameof(PoolID), this.PoolID);
            AddSetting(stringBuilder, nameof(JobID), this.JobID);

            return stringBuilder.ToString();
        }

        private static void AddSetting(StringBuilder stringBuilder, string settingName, object settingValue)
        {
            stringBuilder.AppendFormat("{0} = {1}", settingName, settingValue).AppendLine();
        }
    }
}
