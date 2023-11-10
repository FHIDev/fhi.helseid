using System;
using System.Runtime.Serialization;

namespace Fhi.HelseId.Web;

public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message)
    {

    }
}

[Serializable]
public class InvalidAzureKeyVaultSettingsException : Exception
{
    private const string StandardMessage = "For Azure Key Vault Secret we expect ClientSecret in the format <name of secret>;<uri to vault>. For example: 'MySecret;https://<your-unique-key-vault-name>.vault.azure.net/'";

    public InvalidAzureKeyVaultSettingsException() : base(StandardMessage)
    {
    }

    protected InvalidAzureKeyVaultSettingsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}