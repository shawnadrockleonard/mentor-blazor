using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MentorApp.Data
{
    [SuppressMessage("Minor Code Smell", "S2221:\"Exception\" should not be caught when not required by called methods", Justification = "Catch any error with Azure Key Vault")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Catch any error with Azure Key Vault")]
    [ExcludeFromCodeCoverage]
    public class AzureKeyVaultHelper : IKeyVaultHelper
    {
        private readonly string vault;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly ILogger logger;

        public AzureKeyVaultHelper(IAppSettingEntity appSettings, ILogger<AzureKeyVaultHelper> logger)
        {
            if (appSettings == null)
            {
                throw new ArgumentException("Configuration is missing Application Settings", nameof(appSettings));
            }

            vault = appSettings.AzureKeyVault?.Vault;
            clientId = appSettings.AzureKeyVault?.ClientId;
            clientSecret = appSettings.AzureKeyVault?.ClientSecret;
            this.logger = logger;
        }


        public async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                var secret = await GetKeyVaultClient().GetSecretAsync(vault, secretName);

                return secret.Value;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to retreive {secretName} with {ex?.Message}");
                return null;
            }
        }

        public async Task SetSecretAsync(string secretName, string secretValue)
        {
            try
            {
                await GetKeyVaultClient().SetSecretAsync(vault, secretName, secretValue);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to set {secretName} with {ex?.Message}");
            }
        }

        public static KeyVaultClient GetKeyVaultClientFromManagedIdentity()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            return new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
        }

        private KeyVaultClient GetKeyVaultClient()
        {
            if (!string.IsNullOrEmpty(clientId))
            {
                return new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetAccessTokenAsync));
            }

            return GetKeyVaultClientFromManagedIdentity();
        }

        private async Task<string> GetAccessTokenAsync(string authority, string resource, string scope)
        {
            var clientCredential = new ClientCredential(clientId, clientSecret);
            var authenticationContext = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await authenticationContext.AcquireTokenAsync(resource, clientCredential);

            return result.AccessToken;
        }
    }
}
