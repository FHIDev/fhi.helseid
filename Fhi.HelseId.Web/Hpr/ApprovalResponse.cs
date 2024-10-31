using System.Collections.Generic;

public class ApprovalResponse
{
    public List<Approval> approvals { get; set; } = new List<Approval>();
    public long hpr_number { get; set; }

    public class Approval
    {
        public string profession { get; set; } = string.Empty;
        public Authorization authorization { get; set; } = new Authorization();
        public List<RequisitionRight> requisition_rights { get; set; } = new List<RequisitionRight>();
        public List<string> specialities { get; set; } = new List<string>();
    }

    public class Authorization
    {
        public string value { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }

    public class RequisitionRight
    {
        public string value { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }
}