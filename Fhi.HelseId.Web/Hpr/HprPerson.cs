using System.Collections.Generic;
using Fhi.HelseId.Web.Hpr.Core;

namespace Fhi.HelseId.Web.Hpr
{
    public class HprPerson
    {
        public string HprNummer { get; set; } = string.Empty;
        public bool ErGyldig { get; set; }
        public List<OId9060> Godkjenninger { get; set; } = new List<OId9060>();
    }
}