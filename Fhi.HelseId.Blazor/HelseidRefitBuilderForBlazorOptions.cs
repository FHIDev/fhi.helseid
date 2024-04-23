namespace Fhi.HelseId.Blazor
{
    public class HelseidRefitBuilderForBlazorOptions
    {
        public bool UseCorrelationId { get; set; }

        public Func<IServiceProvider, string>? CustomCorrelationIdFunc { get; set; }

        public bool UseLogoutUrl { get; set; } = true;

        public string LogOutUrl { get; set; } = "/logout";

        public string LoggedOutRedirectUrl { get; set; } = "/loggedout";
    }
}
