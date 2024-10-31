using System.Collections.Generic;

namespace Fhi.HelseId.Web.Hpr
{
    public class UserInfoResponse
    {
        public string Pid { get; set; } = string.Empty;
        public string SecurityLevel { get; set; } = string.Empty;
        public string AssuranceLevel { get; set; } = string.Empty;
        public string PidPseudonym { get; set; } = string.Empty;
        public string HprNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Network { get; set; } = string.Empty;
        public long UserinfoIssuedAt { get; set; }

        public HprDetailsResponse HprDetails { get; set; } = new HprDetailsResponse();

        public string Sub { get; set; } = string.Empty;

        public class HprDetailsResponse
        {
            public List<Approval> Approvals { get; set; } = new List<Approval>();
            public string HprNumber { get; set; } = string.Empty;
        }

        public class Approval
        {
            public string Profession { get; set; } = string.Empty;
            public Authorization Authorization { get; set; } = new Authorization();
            public List<RequisitionRight> RequisitionRights { get; set; } = new List<RequisitionRight>();
            public List<string> Specialities { get; set; } = new List<string>();
        }

        public class Authorization
        {
            public string Value { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        public class RequisitionRight
        {
            public string Value { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

    }
}