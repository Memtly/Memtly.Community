namespace WeddingShare.Models.Database
{
    public class SettingModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Value { get; set; }

        public T Parse<T>(T defaultValue)
        {
            if (!string.IsNullOrWhiteSpace(this.Value))
            { 
                try
                {
                    return (T)Convert.ChangeType(this.Value, typeof(T));
                }
                catch { }
            }

            return defaultValue;
        }
    }
}