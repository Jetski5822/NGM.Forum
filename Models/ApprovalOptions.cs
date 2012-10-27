namespace NGM.Forum.Models {
    public class ApprovalOptions {

        public static ApprovalOptions Approved { get { return new ApprovalOptions { IsApproved = true }; } }

        public static ApprovalOptions NonApproved { get { return new ApprovalOptions { IsApproved = false }; } }

        public static ApprovalOptions All { get { return new ApprovalOptions { IsAll = true }; } }

        public bool IsApproved { get; private set; }
        public bool IsAll { get; private set; }
    }
}