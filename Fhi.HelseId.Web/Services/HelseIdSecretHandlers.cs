using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Fhi.HelseId.Common.Exceptions;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Web.Services
{
    public interface IHelseIdSecretHandler
    {
        void AddSecretConfiguration(OpenIdConnectOptions options);
        string GenerateClientAssertion { get; }
    }

    public abstract class SecretHandlerBase : IHelseIdSecretHandler
    {
        protected abstract SecurityKey GetSecurityKey();

        public const string ClientAssertionType = IdentityModel.OidcConstants.ClientAssertionTypes.JwtBearer;

        protected SecretHandlerBase(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon)
        {
            ConfigAuth = helseIdWebKonfigurasjon;
        }

        public virtual void AddSecretConfiguration(OpenIdConnectOptions options) { }

        public virtual string GenerateClientAssertion => ClientAssertion.Generate(ConfigAuth.ClientId, ConfigAuth.Authority, GetSecurityKey(), ConfigAuth.UseIdPorten);

        protected IHelseIdWebKonfigurasjon ConfigAuth { get; }
    }

    /// <summary>
    /// Used when you have the Jwk in a file. The file should contain the Jwk as a string. The ClientSecret property should contain the file name
    /// </summary>
    public class HelseIdJwkFileSecretHandler : SecretHandlerBase
    {
        private SecurityKey JsonWebKey { get; }
        public HelseIdJwkFileSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
            var jwk = File.ReadAllText(ConfigAuth.ClientSecret);
            JsonWebKey = new JsonWebKey(jwk);
        }

        public override void AddSecretConfiguration(OpenIdConnectOptions options)
        {
            options.Events.OnAuthorizationCodeReceived = ctx =>
            {
                if (ctx.TokenEndpointRequest == null)
                {
                    throw new InvalidOperationException($"{nameof(ctx.TokenEndpointRequest)} cannot be null");
                }

                ctx.TokenEndpointRequest.ClientAssertionType = ClientAssertionType;
                ctx.TokenEndpointRequest.ClientAssertion = GenerateClientAssertion;

                return Task.CompletedTask;
            };

#if NET9_0
            options.Events.OnPushAuthorization = ctx =>
            {
                ctx.ProtocolMessage.ClientAssertionType = ClientAssertionType;
                ctx.ProtocolMessage.ClientAssertion = GenerateClientAssertion;

                return Task.CompletedTask;
            };
#endif
        }

        protected override SecurityKey GetSecurityKey() => JsonWebKey;
    }

    /// <summary>
    /// The ClientSecret property should contain the Jwk private key as a string
    /// </summary>
    public class HelseIdJwkSecretHandler : SecretHandlerBase
    {
        private SecurityKey JsonWebKey { get; }
        public HelseIdJwkSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
            JsonWebKey = new JsonWebKey(ConfigAuth.ClientSecret);
        }

        public override void AddSecretConfiguration(OpenIdConnectOptions options)
        {
            options.Events.OnAuthorizationCodeReceived = ctx =>
            {
                if (ctx.TokenEndpointRequest == null)
                {
                    throw new InvalidOperationException($"{nameof(ctx.TokenEndpointRequest)} cannot be null");
                }

                ctx.TokenEndpointRequest.ClientAssertionType = ClientAssertionType;
                ctx.TokenEndpointRequest.ClientAssertion = GenerateClientAssertion;

                return Task.CompletedTask;
            };

#if NET9_0
            options.Events.OnPushAuthorization = ctx =>
            {
                ctx.ProtocolMessage.ClientAssertionType = ClientAssertionType;
                ctx.ProtocolMessage.ClientAssertion = GenerateClientAssertion;

                return Task.CompletedTask;
            };
#endif
        }

        protected override SecurityKey GetSecurityKey() => JsonWebKey;
    }

    /// <summary>
    /// For Azure Key Vault Secret we expect ClientSecret in the format 'name of secret;uri to vault'. For example: 'MySecret;https://your-unique-key-vault-name.vault.azure.net/'
    /// </summary>
    public class HelseIdJwkAzureKeyVaultSecretHandler : SecretHandlerBase
    {
        private SecurityKey JsonWebKey { get; }
        public HelseIdJwkAzureKeyVaultSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
            var azureClientSettings = ConfigAuth.ClientSecret.Split(';');
            if (azureClientSettings.Length != 2)
            {
                throw new InvalidAzureKeyVaultSettingsException();
            }

            var secretClientOptions = new SecretClientOptions
            {
                Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                }
            };
            var secretClient = new SecretClient(new Uri(azureClientSettings[1]), new DefaultAzureCredential(), secretClientOptions);
            var secret = secretClient.GetSecret(azureClientSettings[0]);

            JsonWebKey = new JsonWebKey(secret.Value.Value);
        }

        public override void AddSecretConfiguration(OpenIdConnectOptions options)
        {
            options.Events.OnAuthorizationCodeReceived = ctx =>
            {
                if (ctx.TokenEndpointRequest == null)
                {
                    throw new InvalidOperationException($"{nameof(ctx.TokenEndpointRequest)} cannot be null");
                }

                ctx.TokenEndpointRequest.ClientAssertionType = ClientAssertionType;
                ctx.TokenEndpointRequest.ClientAssertion = GenerateClientAssertion;

                return Task.CompletedTask;
            };

#if NET9_0
            options.Events.OnPushAuthorization = ctx =>
            {
                ctx.ProtocolMessage.ClientAssertionType = ClientAssertionType;
                ctx.ProtocolMessage.ClientAssertion = GenerateClientAssertion;

                return Task.CompletedTask;
            };
#endif
        }

        protected override SecurityKey GetSecurityKey() => JsonWebKey;
    }

    public class HelseIdEnterpriseCertificateSecretHandler : SecretHandlerBase
    {
        private readonly X509SecurityKey _x509SecurityKey;
        public HelseIdEnterpriseCertificateSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
            var secretParts = ConfigAuth.ClientSecret.Split(':');
            if (secretParts.Length != 2)
            {
                throw new InvalidEnterpriseCertificateSecretException(ConfigAuth.ClientSecret);
            }

            var storeLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), secretParts[0]);
            var thumbprint = secretParts[1];

            var store = new X509Store(storeLocation);
            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);

            if (certificates.Count == 0)
            {
                throw new Exception($"No certificate with thumbprint {thumbprint} found in store LocalMachine");
            }

            _x509SecurityKey = new X509SecurityKey(certificates[0]);
        }

        public override void AddSecretConfiguration(OpenIdConnectOptions options)
        {
            options.Events.OnAuthorizationCodeReceived = ctx =>
            {
                if (ctx.TokenEndpointRequest == null)
                {
                    throw new InvalidOperationException($"{nameof(ctx.TokenEndpointRequest)} cannot be null");
                }

                ctx.TokenEndpointRequest.ClientAssertionType = ClientAssertionType;
                ctx.TokenEndpointRequest.ClientAssertion = GenerateClientAssertion;

                return Task.CompletedTask;
            };

#if NET9_0
            options.Events.OnPushAuthorization = ctx =>
            {
                ctx.ProtocolMessage.ClientAssertionType = ClientAssertionType;
                ctx.ProtocolMessage.ClientAssertion = GenerateClientAssertion;

                return Task.CompletedTask;
            };
#endif
        }

        public class InvalidEnterpriseCertificateSecretException : Exception
        {
            private const string StandardMessage = "For enterprise certificates we expect secret in the format STORE:Thumbprint. For example: 'LocalMachine:1234567890'";

            public InvalidEnterpriseCertificateSecretException(string secret) : base(StandardMessage)
            {
                Secret = secret;
            }

            public string Secret { get; }
        }

        protected override SecurityKey GetSecurityKey() => _x509SecurityKey;
    }

    public class HelseIdNoAuthorizationSecretHandler : SecretHandlerBase
    {

        public HelseIdNoAuthorizationSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
        }

        /// <summary>
        /// Will never be called
        /// </summary>
        /// <returns></returns>
        protected override SecurityKey GetSecurityKey()
        {
            throw new NotImplementedException("This method is not used and should not be called. If this happens, there is a missing check for AuthUse");
        }
    }

    /// <summary>
    /// For selvbetjening we expect ClientSecret to be a path to a file containing the full downloaded configuration file, including the private key in JWK format
    /// </summary>
    public class HelseIdSelvbetjeningSecretHandler : SecretHandlerBase
    {
        private SecurityKey JsonWebKey { get; }
        public HelseIdSelvbetjeningSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
            var selvbetjeningJson = File.ReadAllText(ConfigAuth.ClientSecret);
            var selvbetjeningConfig = JsonSerializer.Deserialize<SelvbetjeningConfig>(selvbetjeningJson);

            if (selvbetjeningConfig == null)
            {
                throw new MissingConfigurationException($"{nameof(SelvbetjeningConfig)}: Could not deserialize json or path not found.");
            }

            var jwk = HttpUtility.UrlDecode(selvbetjeningConfig.PrivateJwk);

            JsonWebKey = new JsonWebKey(jwk);
        }

        public override void AddSecretConfiguration(OpenIdConnectOptions options)
        {
            options.Events.OnAuthorizationCodeReceived = ctx =>
            {
                if (ctx.TokenEndpointRequest == null)
                {
                    throw new InvalidOperationException($"{nameof(ctx.TokenEndpointRequest)} cannot be null");
                }

                ctx.TokenEndpointRequest.ClientAssertionType = ClientAssertionType;
                ctx.TokenEndpointRequest.ClientAssertion = GenerateClientAssertion;

                return Task.CompletedTask;
            };

#if NET9_0
            options.Events.OnPushAuthorization = ctx =>
            {
                ctx.ProtocolMessage.ClientAssertionType = ClientAssertionType;
                ctx.ProtocolMessage.ClientAssertion = GenerateClientAssertion;

                return Task.CompletedTask;
            };
#endif
        }

        public class SelvbetjeningConfig
        {
            [JsonPropertyName("clientName")]
            public string? ClientName { get; set; }
            [JsonPropertyName("authority")]
            public string? Authority { get; set; }
            [JsonPropertyName("clientId")]
            public string? ClientId { get; set; }
            [JsonPropertyName("grantTypes")]
            public string[]? GrantTypes { get; set; }
            [JsonPropertyName("scopes")]
            public string[]? Scopes { get; set; }
            [JsonPropertyName("redirectUris")]
            public string[]? RedirectUris { get; set; }
            [JsonPropertyName("postLogoutRedirectUris")]
            public object[]? PostLogoutRedirectUris { get; set; }
            [JsonPropertyName("secretType")]
            public string? SecretType { get; set; }
            [JsonPropertyName("rsaPrivateKey")]
            public string? RsaPrivateKey { get; set; }
            [JsonPropertyName("rsaKeySizeBits")]
            public int RsaKeySizeBits { get; set; }
            [JsonPropertyName("privateJwk")]
            public string? PrivateJwk { get; set; }
            [JsonPropertyName("pkceRequired")]
            public bool PkceRequired { get; set; }
        }

        protected override SecurityKey GetSecurityKey() => JsonWebKey;
    }
}