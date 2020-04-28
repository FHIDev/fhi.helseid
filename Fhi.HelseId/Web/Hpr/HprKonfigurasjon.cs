namespace Fhi.HelseId.Web.Hpr
{
    public interface IHprFeatureFlags
    {
        bool UseHpr { get; set; }
        bool UseHprPolicy { get; set; }
        int Validate();
    }

    public class HprKonfigurasjon : IHprFeatureFlags
    {
        public bool UseHpr { get; set; } = true;

        public bool UseHprPolicy { get; set; } = true;
        public string Brukernavn { get; set; } = "";
        public string Passord { get; set; } = "";

        public string Url { get; set; } = "";

        public int Validate()
        {
            if (!UseHpr)
                return 0; // Ok, anvender ikke Hpr
            if (string.IsNullOrEmpty(Url))
                return -1;
            if (string.IsNullOrEmpty(Brukernavn))
                return -2;
            if (string.IsNullOrEmpty(Passord))
                return -3;
            if (!UseHprPolicy)
                return 2;  // Ok, men anvender ikke HprPolicy
            return 1; // Ok, anvender alt
        }

    }
}
