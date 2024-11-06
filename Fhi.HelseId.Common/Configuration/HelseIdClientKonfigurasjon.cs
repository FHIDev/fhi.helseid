using Fhi.HelseId.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Fhi.HelseId.Common.Configuration
{
    public interface IHelseIdClientKonfigurasjon : IAutentiseringkonfigurasjon
    {
        bool UseHttps { get; set; }
        bool RewriteRedirectUriHttps { get; set; }
        string Authority { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string[] Scopes { get; set; }
        bool Debug { get; set; }
        IEnumerable<string> AllScopes { get; }
        string JsonWebKeySecret { get; set; }
        string RsaKeySecret { get; set; }
        Whitelist Whitelist { get; set; }
    }

    public abstract class HelseIdClientKonfigurasjon : HelseIdCommonKonfigurasjon, IHelseIdClientKonfigurasjon
    {
        public bool RewriteRedirectUriHttps { get; set; } = false;
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string[] Scopes { get; set; } = [];
        public bool Debug { get; set; } = false;
        public List<string> HprScope { get; set; } = new List<string>();

        public IEnumerable<string> AllScopes => BaseScopes.Concat(Scopes).Concat(HprScope).Distinct();
        
        public virtual IEnumerable<string> BaseScopes { get; set; } = [];

        public string JsonWebKeySecret { get; set; } = "";
        public string RsaKeySecret { get; set; } = "";

        public Whitelist Whitelist { get; set; } = new();
    }
}