namespace Fhi.HelseId.Web.Hpr
{
    public class HprKonfigurasjon
    {
        public bool UseHpr { get; set; } = true;

        public bool UseHprPolicy { get; set; } = true;
        public string Brukernavn { get; set; } = "";
        public string Passord { get; set; } = "";

        public string Url { get; set; } = "";
    }
}
