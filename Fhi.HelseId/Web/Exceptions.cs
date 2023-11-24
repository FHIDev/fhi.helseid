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

[Serializable]
public class MissingConfigurationException : Exception
{
    public MissingConfigurationException(string outgoingApisName) : base(outgoingApisName)
    {

    }
}


[Serializable]
public class InvalidApiNameException : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public InvalidApiNameException() { }
    public InvalidApiNameException(string message) : base(message) { }
    public InvalidApiNameException(string message, Exception inner) : base(message, inner) { }

    protected InvalidApiNameException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}