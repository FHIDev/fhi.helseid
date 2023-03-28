using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Fhi.HelseId.Common;
using Fhi.HelseId.Web.Hpr;
using Fhi.HelseId.Web.Services;

namespace Fhi.HelseId.Web
{
    public interface IHelseIdHprFeatures
    {
        bool UseHprNumber { get; }
        bool UseHprPolicy { get; }
        string HprUsername { get; set; }
        string HprPassword { get; set; }
        string HprUrl { get; set; }
    }

    public interface IHelseIdWebKonfigurasjon : IHelseIdHprFeatures, IHelseIdClientKonfigurasjon
    {

        string[] SecurityLevels { get; }
        bool UseProtectedPaths { get; set; }
        RedirectPagesKonfigurasjon RedirectPagesKonfigurasjon { get; set; }
        bool UseApis { get; set; }
    }


    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public class HelseIdWebKonfigurasjon : HelseIdClientKonfigurasjon, IHelseIdWebKonfigurasjon
    {
        

        public string[] SecurityLevels { get; set; } = { "3", "4" };
    
        
        protected override IEnumerable<string> FixedScopes
        {
            get
            {
                var list =  new List<string>
                {
                    "openid",
                    "profile",
                    "helseid://scopes/identity/pid",
                    "helseid://scopes/identity/pid_pseudonym",
                    "helseid://scopes/identity/security_level",
                    "helseid://scopes/hpr/hpr_number"
                };
                list.AddRange(base.FixedScopes);
                return list;
            }
        }

        public bool UseHpr { get; set; } = false;
        public bool UseHprNumber { get; set; } = false;
        public bool UseHprPolicy { get; set; } = false;

        public string HprUsername { get; set; } = "";
        public string HprPassword { get; set; } = "";

        public string HprUrl { get; set; } = "";

        public bool UseProtectedPaths { get; set; } = false;

        public bool UseApis { get; set; } = false;

        public RedirectPagesKonfigurasjon RedirectPagesKonfigurasjon { get; set; } = new();

        public int Validate()
        {
            throw new NotImplementedException();
        }
    }
}
