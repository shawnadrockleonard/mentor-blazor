namespace MentorApp.Data
{
    public class AppSettingEntity : IAppSettingEntity
    {
        public virtual AzureAdOptions AzureAd { get; set; }

        public virtual AzureKeyValueEntity AzureKeyVault { get; set; }

        public virtual ConnectionStringEntity ConnectionStrings { get; set; }
    }
}