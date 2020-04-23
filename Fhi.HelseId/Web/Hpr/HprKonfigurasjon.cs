namespace Fhi.HelseId.Web.Hpr
{
    public interface IHprFeatureFlags
    {
        bool UseHpr { get; set; }
        bool UseHprPolicy { get; set; }
    }

    public class HprKonfigurasjon : IHprFeatureFlags
    {
        public bool UseHpr { get; set; } = true;

        public bool UseHprPolicy { get; set; } = true;
        public string Brukernavn { get; set; } = "";
        public string Passord { get; set; } = "";

        public string Url { get; set; } = "";
    }
}
