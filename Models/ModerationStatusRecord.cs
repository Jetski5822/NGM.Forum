using Orchard.Data.Conventions;

namespace NGM.Forum.Models {
    public class ModerationStatusRecord {
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        [StringLengthMax]
        public virtual string Text { get; set; }
    }
}