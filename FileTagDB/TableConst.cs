
namespace FileTagDB {
    public static class TableConst {
        // T is table Co is column
        #region Files Table
        public const string filesTName = "files";
        public const string filesCoID = "fid";
        public const string filesCoPath = "path";
        #endregion

        #region Tags Table
        public const string tagsTName = "tags";
        public const string tagsCoID = "tid";
        public const string tagsCoName = "name";
        #endregion

        public const string fileTagsTName = "file_tags";
        public const string fileTagsCoFID = "fid";
        public const string fileTagsCoTID = "tid";

        public const string fileChildsTName = "file_childs";
        public const string fileChildsFID = "fid";
        public const string fileChildsCID = "cid";
        public static readonly string[] allTables = {filesTName, tagsTName, fileTagsTName, fileChildsTName};
        public const string fileChildsConstraintName = "check_fid_cid";

    }
}
