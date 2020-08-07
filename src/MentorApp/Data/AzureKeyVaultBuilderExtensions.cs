using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using System;

namespace MentorApp.Data
{
    public static class AzureKeyVaultBuilderExtensions
    {
        private const string AzureKeyVaultKey = "AzureKeyVault";
        private const string AzureKeyVaultUrlKey = "Vault";

        /// <summary>
        /// if Azure Key Value is available, reads configuration values from the Azure KeyVault.
        /// </summary>
        /// <param name="builder"><see cref="IConfigurationBuilder"/></param>
        public static IConfigurationBuilder AddAzureKeyVaultIfAvailable(this IConfigurationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var configurationRoot = builder.Build();
            var keyVaultConfiguration = configurationRoot.GetSection(AzureKeyVaultKey);
            string clientId = keyVaultConfiguration["ClientId"];
            string vaultUrl = keyVaultConfiguration[AzureKeyVaultUrlKey];

            if (string.IsNullOrEmpty(vaultUrl))
            {
                return builder;
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                // Try to access the Key Vault utilizing the Managed Service Identity of the running resource/process                              
                builder.AddAzureKeyVault(vaultUrl, AzureKeyVaultHelper.GetKeyVaultClientFromManagedIdentity(), new DefaultKeyVaultSecretManager());
            }
            else
            {
                // Allow to override the MSI or for local dev
                builder.AddAzureKeyVault(vaultUrl, clientId, keyVaultConfiguration["ClientSecret"]);
            }

            return builder;
        }
    }
}
