using System;

namespace NGM.Forum.Models {
    public class ModerationOptions : IEquatable<ModerationOptions> {

        public static ModerationOptions Approved { get { return new ModerationOptions { IsApproved = true }; } }

        public static ModerationOptions AwaitingModeration { get { return new ModerationOptions { IsAwaitingApproval = true }; } }

        public static ModerationOptions NotApproved { get { return new ModerationOptions { IsApproved = false }; } }

        public static ModerationOptions All { get { return new ModerationOptions { IsAll = true }; } }

        public bool IsApproved { get; private set; }
        public bool IsAwaitingApproval { get; private set; }
        public bool IsAll { get; private set; }

        public bool Equals(ModerationOptions other) {
            if (this.IsAll != other.IsAll) {
                return false;
            }
            if (this.IsAwaitingApproval != other.IsAwaitingApproval) {
                return false;
            }
            if (this.IsApproved != other.IsApproved) {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj) {
            ModerationOptions moderationOptions = obj as ModerationOptions;

            if (moderationOptions == null)
                return false;

            return Equals(moderationOptions);
        }
    }
}