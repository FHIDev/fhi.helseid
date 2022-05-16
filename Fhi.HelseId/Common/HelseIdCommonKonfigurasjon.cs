namespace Fhi.HelseId.Common
{
    public abstract class HelseIdCommonKonfigurasjon
    {
        public string Authority { get; set; } = "";
        public bool AuthUse { get; set; } = true;
        public bool UseHttps { get; set; } = true;
    }
}