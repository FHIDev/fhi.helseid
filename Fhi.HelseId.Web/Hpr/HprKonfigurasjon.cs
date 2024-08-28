namespace Fhi.HelseId.Web.Hpr;



public class HprKonfigurasjon 
{
    
    public bool UseHprPolicy { get; set; } = true;
    public string Brukernavn { get; set; } = "";
    public string Passord { get; set; } = "";

    public string Url { get; set; } = "";

    public int Validate()
    {
        //if (!UseHprNumber)
        //    return 0; // Ok, anvender ikke Hpr
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

    public HprKonfigurasjon()
    {
            
    }

    public HprKonfigurasjon(IHelseIdHprFeatures hprFeaturflags)
    {
        UseHprPolicy = hprFeaturflags.UseHprPolicy;
    }
        

}