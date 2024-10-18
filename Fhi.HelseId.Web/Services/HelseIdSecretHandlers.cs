using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
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
        protected JsonWebKey? jwkSecurityKey;

        public const string ClientAssertionType = IdentityModel.OidcConstants.ClientAssertionTypes.JwtBearer;

        protected SecretHandlerBase(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon)
        {
            ConfigAuth = helseIdWebKonfigurasjon;
        }

        public virtual void AddSecretConfiguration(OpenIdConnectOptions options) { }

        public virtual string GenerateClientAssertion => ClientAssertion.Generate(ConfigAuth.ClientId, ConfigAuth.Authority, jwkSecurityKey);

        protected IHelseIdWebKonfigurasjon ConfigAuth { get; private set; }
    }

    /// <summary>
    /// Used when you have the Jwk in a file. The file should contain the Jwk as a string. The ClientSecret property should contain the file name
    /// </summary>
    public class HelseIdJwkFileSecretHandler : SecretHandlerBase
    {
        public HelseIdJwkFileSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
        }

        public override void AddSecretConfiguration(OpenIdConnectOptions options)
        {
            var jwk = File.ReadAllText(ConfigAuth.ClientSecret);
            jwkSecurityKey = new JsonWebKey(jwk);

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
    }

    /// <summary>
    /// The ClientSecret property should contain the Jwk private key as a string
    /// </summary>
    public class HelseIdJwkSecretHandler : SecretHandlerBase
    {
        public HelseIdJwkSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
        }

        public override void AddSecretConfiguration(OpenIdConnectOptions options)
        {
            jwkSecurityKey = new JsonWebKey(ConfigAuth.ClientSecret);

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
    }

    /// <summary>
    /// For Azure Key Vault Secret we expect ClientSecret in the format 'name of secret;uri to vault'. For example: 'MySecret;https://your-unique-key-vault-name.vault.azure.net/'
    /// </summary>
    public class HelseIdJwkAzureKeyVaultSecretHandler : SecretHandlerBase
    {
        public HelseIdJwkAzureKeyVaultSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
        }

        public override void AddSecretConfiguration(OpenIdConnectOptions options)
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

            jwkSecurityKey = new JsonWebKey(secret.Value.Value);

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
    }

    /// <summary>
    /// Don't use this
    /// </summary>
    public class HelseIdRsaXmlSecretHandler : SecretHandlerBase
    {
        private RsaSecurityKey _rsaSecurityKey;

        public HelseIdRsaXmlSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
        }

        public override string GenerateClientAssertion => ClientAssertion.Generate(ConfigAuth.ClientId, ConfigAuth.Authority, _rsaSecurityKey);

        public override void AddSecretConfiguration(OpenIdConnectOptions options)
        {
            var xml = File.ReadAllText(ConfigAuth.ClientSecret);
            var rsa = RSA.Create();
            rsa.FromXmlString(xml);
            _rsaSecurityKey = new RsaSecurityKey(rsa);

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
    }

    public class HelseIdEnterpriseCertificateSecretHandler : SecretHandlerBase
    {
        private X509SecurityKey _x509SecurityKey;

        public HelseIdEnterpriseCertificateSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
        }

        public override string GenerateClientAssertion => ClientAssertion.Generate(ConfigAuth.ClientId, ConfigAuth.Authority, _x509SecurityKey);

        public override void AddSecretConfiguration(OpenIdConnectOptions options)
        {
            var secretParts = ConfigAuth.ClientSecret.Split(':');
            if (secretParts.Length != 2)
            {
                throw new InvalidEnterpriseCertificateSecretException(ConfigAuth.ClientSecret);
            }

            var storeLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), secretParts[0]);
            var thumprint = secretParts[1];

            var store = new X509Store(storeLocation);
            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumprint, true);

            if (certificates.Count == 0)
            {
                throw new Exception($"No certificate with thumbprint {options.ClientSecret} found in store LocalMachine");
            }

            _x509SecurityKey = new X509SecurityKey(certificates[0]);

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
    }

    public class HelseIdSharedSecretHandler : SecretHandlerBase
    {
        public HelseIdSharedSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
        }

        public override void AddSecretConfiguration(OpenIdConnectOptions options)
        {
            options.ClientSecret = ConfigAuth.ClientSecret;
        }
    }

    public class HelseIdNoAuthorizationSecretHandler : SecretHandlerBase
    {
        public HelseIdNoAuthorizationSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
        }
    }

    /// <summary>
    /// For selvbetjening we expect ClientSecret to be a path to a file containing the full downloaded configuration file, including the private key in JWK format
    /// </summary>
    public class HelseIdSelvbetjeningSecretHandler : SecretHandlerBase
    {
        public HelseIdSelvbetjeningSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
        }

        public override void AddSecretConfiguration(OpenIdConnectOptions options)
        {
            var selvbetjeningJson = File.ReadAllText(ConfigAuth.ClientSecret);

            var selvbetjeningConfig = JsonSerializer.Deserialize<SelvbetjeningConfig>(selvbetjeningJson);
            var jwk = HttpUtility.UrlDecode(selvbetjeningConfig.PrivateJwk);

            jwkSecurityKey = new JsonWebKey(jwk);

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
    }
}