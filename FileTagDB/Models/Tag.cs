namespace FileTagDB.Models {
    public class Tag : IComparable<Tag> {
        public int id;
        public string name;
        public Tag(int p_id, string p_name) {
            id = p_id;
            name = p_name;
        }

        int IComparable<Tag>.CompareTo(Tag? other) {
            if (other == null)
                return 1;
            return name.CompareTo(other.name);
        }
    }
}
