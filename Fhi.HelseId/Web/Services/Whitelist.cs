using System;
using System.Collections.Generic;
using System.Linq;
using Fhi.HelseId.Common.Identity;

namespace Fhi.HelseId.Web.Services
{
    public interface IWhitelist
    {
        bool IsWhite(ICurrentUser user);
        bool IsWhite(string pidPseudonym);

        string NameOf(ICurrentUser user);
    }

    public class Whitelist : List<White>, IWhitelist
    {
        public bool IsWhite(ICurrentUser user) => this.Any(x=>x.PidPseudonym.Equals(user.PidPseudonym,StringComparison.InvariantCultureIgnoreCase));

        public bool IsWhite(string pidPseudonym) => this.Any(x =>
            x.PidPseudonym.Equals(pidPseudonym, StringComparison.InvariantCultureIgnoreCase));

        public string NameOf(ICurrentUser user)
        {
            var result = this.FirstOrDefault(x =>
                x.PidPseudonym.Equals(user.PidPseudonym, StringComparison.InvariantCultureIgnoreCase));
            return result?.Name ?? "";
        }
    }
}
