using System.Data.SQLite;
using System.Text;
using System.Text.RegularExpressions;

namespace FileTagDB.Controllers {
    public partial class TagController {
        // NOTE: We will not handle ensuring file IDs here
        public void TagFile(int tagId, int fileId) {
            ConnectDB();
            TagFileConnected(tagId, fileId);
            DisconnectDB();
        }
        public void TagFiles(int tagId, List<int> fileIds) { // files are assumed to be in DB
            ConnectDB();
            TagFilesConnected(tagId, fileIds);
            DisconnectDB();
            // get all IDs, then untag them
        }
        public void UntagFile(int tagId, int fileId) {
            ConnectDB();
            UntagFileConnected(tagId, fileId);
            DisconnectDB();
        }
        public void UntagFiles(int tagId, List<int> fileIds) {
            ConnectDB();
            UntagFilesConnected(tagId, fileIds);
            DisconnectDB();
        }





        public void RemoveFilesWithoutTags() {
            ConnectDB();
            ActivateForeignKey();
            RemoveFilesWithoutTagsConnected();
            DisconnectDB();
        }
        public List<int> GetFileTags(string file) {
            ConnectDB();
            _fileController.conn = conn;
            List<int> fileTags = GetFileTagsConnected(_fileController.GetFileIDDBC(file));
            DisconnectDB();
            return fileTags;
        }
        public List<int> GetFileTags(int fileId) {
            ConnectDB();
            List<int> fileTags = GetFileTagsConnected(fileId);
            DisconnectDB();
            return fileTags;
        }

        public List<List<int>> GetFilesTags(List<string> files) {
            ConnectDB();
            _fileController.conn = conn;
            List<List<int>> filesTags= GetFilesTagsConnected(_fileController.GetFilesIds(files));
            DisconnectDB();
            return filesTags;
        }
        public List<List<int>> GetFilesTags(List<int> fileIds) {
            ConnectDB();
            List<List<int>> filesTags= GetFilesTagsConnected(fileIds);
            DisconnectDB();
            return filesTags;
        }

        public List<(string,int)> GetFilesWithTag(int tagId) {
            List<(string, int)> fileRows = new();
            ConnectDB();
            using (var cmd = new SQLiteCommand(conn)) {
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd, 
                    @$"SELECT filep.* FROM {TableConst.filesTName} filep JOIN
                    {TableConst.fileTagsTName} ft ON
                    ft.{TableConst.fileTagsCoFID} = filep.{TableConst.filesCoID}
                    WHERE ft.{TableConst.fileTagsCoTID} = {tagId}
                "); // direct child only!
                while (reader.Read())
                    fileRows.Add(new((string)reader[$"{TableConst.filesCoPath}"], Convert.ToInt32(reader[$"{TableConst.filesCoID}"])));
            }
            DisconnectDB();
            return fileRows;
        }

        public List<(string,int)>GetFilesWithTagQuery(string tagQuery, bool ignoreCase) {
            Utils.LogToOutput("Original query " + tagQuery);
            string idPrefix = $"ft.{TableConst.fileTagsCoTID}";
            tagQuery = AdjustQuery(tagQuery, idPrefix, TableConst.tagsTName, TableConst.tagsCoID, TableConst.tagsCoName);
            Utils.LogToOutput("Translated query " + tagQuery);
            List<(string, int)> fileRows = new();
            ConnectDB();
            if(!ignoreCase)
                UseCaseSensitiveLike();
            using (var cmd = new SQLiteCommand(conn)) {
                //string oldCmdText = @$"SELECT DISTINCT f.* FROM {TableConst.filesTName} f
                //    JOIN {TableConst.fileTagsTName} ft ON  f.{TableConst.filesCoID} = ft.{TableConst.fileTagsCoFID}
                //    WHERE {tagQuery}
                //";

                string cmdText = JoinQueryAsIntersect(@$"SELECT DISTINCT f.* FROM {TableConst.filesTName} f JOIN {TableConst.fileTagsTName} ft ON  f.{TableConst.filesCoID} = ft.{TableConst.fileTagsCoFID} WHERE ",
                    tagQuery, idPrefix);
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd, cmdText); // direct child only!
                while (reader.Read()) {
                    fileRows.Add(new((string)reader[$"{TableConst.filesCoPath}"], Convert.ToInt32(reader[$"{TableConst.filesCoID}"])));
                }
            }
            DisconnectDB();
            return fileRows;
        }

        // test searching for tags with DB characters where * means '%' and ~ means '_'
        //  and user given '%' and '_' and '/' are all escaped with '/" character first
        // So, escapify characters, then replace with suitable
        // Note we assume here the string is correctly formatted
        // NOTE: You CANNOT have -a+b , user can't use - ( ) + * ~ for tag names
        const string endingAnd = " AND";
        private string AdjustQuery(string tagQuery, string idPrefixed, string tableName, string idCol, string nameCol) {
            if (tagQuery.Contains("-") && !Regex.IsMatch(tagQuery, @"\s+")) // so only one negative query
                tagQuery = "* " + tagQuery;
            
            int addedStringLength = $@" {idPrefixed} IN (SELECT {idCol} FROM {tableName} WHERE {nameCol} LIKE SOMETHING ESCAPE '\'){endingAnd}".Length;
            StringBuilder sb = new StringBuilder();
            // escape the escape character first, then escape the DB characters
            tagQuery = tagQuery.Replace(@"\", @"\\").Replace("'","''").Replace("_", @"\_").Replace("%", @"\%")
                .Replace('*','%').Replace('~','_');

            string[] parts = Regex.Split(tagQuery, @"\s+");
            // if all of them are negative, add 1 positive %


            // a good approximation
            sb.EnsureCapacity(tagQuery.Length + parts.Length * addedStringLength + (7 + addedStringLength) * (Utils.Count(tagQuery, '+')) + 3000); // ( OR )
            bool allNegative = true;
            for (int q1 = 0; q1 < parts.Length; q1++) {
                string part = parts[q1];
                if (part[0] != '-')
                    allNegative = false;
                AddQuery(sb, part, idPrefixed, tableName, idCol, nameCol);
            }
            if (allNegative)
                AddQuery(sb, "%", idPrefixed, tableName, idCol, nameCol);
            sb.Length -= 4;
            return Regex.Replace(sb.ToString(), @"\s+", " ");
        }
        private void AddQuery(StringBuilder sb, string part, string idPrefixed, string tableName, string idCol, string nameCol) {
            if (part.Contains('+')) {
                string[] orGroup = part.Split('+');
                sb.Append($" {idPrefixed} IN (SELECT {idCol} FROM {tableName} WHERE ");
                for (int i = 0; i < orGroup.Length; i++)
                    orGroup[i] = $"{nameCol} LIKE '{orGroup[i]}' ESCAPE '\\'";
                sb.Append(string.Join(" OR ", orGroup));
                sb.Append(")");
                sb.Append(endingAnd);
            } else {
                string addNot = string.Empty;
                if (part[0] == '-') {
                    part = part.Substring(1);
                    addNot = "NOT";
                }
                sb.Append($@" {idPrefixed} {addNot} IN (SELECT {idCol} FROM {tableName} WHERE {nameCol} LIKE '{part}' ESCAPE '\'){endingAnd}");
            }
        }

        // INTERSECT has precedence over EXCEPT
        // The result of the INTERSECT operation between b and c will be subtracted from a when using the query a EXCEPT b INTERSECT c.
        private string JoinQueryAsIntersect(string selectPart, string queryConditions, string idPrefixed) {
            StringBuilder sb = new();
            string[] conditions = queryConditions.Split(endingAnd);
            int notLength = $" {idPrefixed} NOT ".Length;
            foreach(string condition in conditions) { // regular loop
                if (!condition.StartsWith($" {idPrefixed} NOT ")) {
                    sb.Append(selectPart);
                    sb.Append(condition);
                }
                sb.Append(" INTERSECT ");
            }
            sb.Length -= 11;// " INTERSECT ".Length;
            foreach (string condition in conditions) { // regular loop
                if (condition.StartsWith($" {idPrefixed} NOT ")) {
                    sb.Length -= 11;
                    sb.Append(" EXCEPT ");
                    sb.Append(selectPart);
                    sb.Append($" {idPrefixed} " + condition.Substring(notLength));
                }
            }
            Utils.LogToOutput("New query : " + sb.ToString());
            return sb.ToString();
        }

    }



    //* 
    //* SELECT p.*
    //* FROM Pizza p
    //* JOIN PT pt ON p.ID = pt.PizzaID
    //* WHERE pt.ToppingID IN (SELECT ID FROM Toppings WHERE name LIKE 't%')
    //*      AND pt.ToppingID IN (SELECT ID FROM Toppings WHERE name IN ('cheese', 'sauce'));
    //* 
    //* 
}
