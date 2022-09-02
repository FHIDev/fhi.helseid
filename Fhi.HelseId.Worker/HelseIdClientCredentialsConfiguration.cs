namespace Fhi.HelseId.Worker;

public partial class HelseIdClientCredentialsConfiguration
{
    public string Authority => authority;
    public string ClientId => clientId;
    public string Scopes
    {
        get
        {
            if (scopes == null)
                return "";
            return string.Join(" ", scopes);
        }
    }

    public string PrivateKey => privateJwk;

    public string Url => url;
}