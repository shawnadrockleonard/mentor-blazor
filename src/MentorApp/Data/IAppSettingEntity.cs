namespace MentorApp.Data
{
    public interface IAppSettingEntity
    {
        AzureAdOptions AzureAd { get; set; }
        AzureKeyValueEntity AzureKeyVault { get; set; }

        ConnectionStringEntity ConnectionStrings { get; set; }
    }
}