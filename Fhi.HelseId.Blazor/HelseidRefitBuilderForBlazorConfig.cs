namespace Fhi.HelseId.Blazor
{
    public class HelseidRefitBuilderForBlazorConfig
    {
        public bool UseCorrelationId { get; set; }

        public bool UseLogoutUrl { get; set; }

        public string LogOutUrl { get; set; } = "/logout";

        public string LoggedOutRedirectUrl { get; set; } = "/loggedout";
    }
}
