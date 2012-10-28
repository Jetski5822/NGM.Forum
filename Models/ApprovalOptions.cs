using System;

namespace NGM.Forum.Models {
    public class ApprovalOptions : IEquatable<ApprovalOptions> {

        public static ApprovalOptions Approved { get { return new ApprovalOptions { IsApproved = true }; } }

        public static ApprovalOptions NonApproved { get { return new ApprovalOptions { IsApproved = false }; } }

        public static ApprovalOptions All { get { return new ApprovalOptions { IsAll = true }; } }

        public bool IsApproved { get; private set; }
        public bool IsAll { get; private set; }

        public bool Equals(ApprovalOptions other) {
            if (this.IsAll != other.IsAll) {
                return false;
            }
            if (this.IsApproved != other.IsApproved) {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj) {
            ApprovalOptions approvalOptions = obj as ApprovalOptions;

            if (approvalOptions == null)
                return false;

            return Equals(approvalOptions);
        }
    }
}