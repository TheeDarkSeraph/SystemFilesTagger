using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FileTagDB.Models;

namespace FileTagDB.Controllers {
    // TODO: NOTE: Tags are assumed to never reach 1 mil tags, but will never exceed that so
    //      all tags will be loaded in memory along with their IDs

    public partial class TagController {
        // TODO: Renamable box selection item...
        // TODO: Select from list where tag = regexp (translated FROM user)
        FileController _fileController;
        SQLiteConnection conn;
        public TagController(FileController fc) {
            _fileController = fc;
            conn = DBController.GetDBConnection();
        }
        private void ConnectDB() {
            conn = DBController.GetDBConnection();
            conn.Open();
        }
        private void DisconnectDB() {
            conn.Dispose();
        }

        #region Create tags
        public int CreateTag(string tag) {
            int lastInsertedRowId = -1;
            ConnectDB();
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$name", tag);
                if (-1 == DBController.ExecuteNonQCommand(cmd, $"INSERT INTO {TableConst.tagsTName} ({TableConst.tagsCoName}) VALUES ($name);"))
                    return -1;
                cmd.CommandText = "SELECT last_insert_rowid();";
                Int64 LastRowID64 = (Int64)cmd.ExecuteScalar();
                lastInsertedRowId = (int)LastRowID64;
                // The row ID is a 64-bit value - cast the Command result to an Int64.
                //
            }
            DisconnectDB();
            return lastInsertedRowId;
        }
        public int CreateTags(List<string>tags) {
            int tagInserted = 0;
            ConnectDB();
            using (var transaction = conn.BeginTransaction()) {
                foreach (string tag in tags) {
                    var command = conn.CreateCommand(); // we create a new command each time because a unique error cause it to permenantly fail
                    command.CommandText = @$"INSERT INTO {TableConst.tagsTName} ({TableConst.tagsCoName}) VALUES ($name);";
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "$name";
                    command.Parameters.Add(parameter);
                    try {
                        parameter.Value = tag;
                        command.ExecuteNonQuery();
                        tagInserted++;
                    } catch (Exception) {}
                }
                transaction.Commit();
            }
            DisconnectDB();
            return tagInserted;
        }
        #endregion

        #region Get Tag or their info
        public int CountTags() {
            string cmdText = $"SELECT COUNT({TableConst.tagsCoName}) FROM {TableConst.tagsTName};";
            object result = DBController.ExecuteScalerAutoConn(cmdText);
            return Convert.ToInt32(result);
        }
        public string GetTagName(int tagID) {
            object? result;
            string cmdText = $"SELECT {TableConst.tagsCoName} FROM {TableConst.tagsTName} WHERE {TableConst.tagsCoID} = {tagID};";
            bool success;
            ConnectDB();
            using (var cmd = new SQLiteCommand(conn))
                success = DBController.TryExecuteSingleRead(cmd, cmdText, $"{TableConst.tagsCoName}", out result);
            DisconnectDB();
            if (!success || result == null)
                return "+NotFound";
            return (string)result;
        }
        // won't be used, tags will be kept in memory prolly for faster processing of user requests and suggestions, 1 mil of average 7 chars is 11MB roughly which is still nothing
        // there isn't even 1 mil words...
        public int GetTagID(string tag) {
            object? result;
            string cmdText = $"SELECT  {TableConst.tagsCoID} FROM {TableConst.tagsTName} WHERE {TableConst.tagsCoName} = $tag";
            bool success;
            ConnectDB();
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$tag", tag);
                success = DBController.TryExecuteSingleRead(cmd, cmdText, $"{TableConst.tagsCoID}", out result);
            }
            DisconnectDB();
            if (!success||result==null)
                return -1;
            return (int)((Int64)result);
        }

        public List<Tag> GetAllTags() {
            List<Tag> tags = new();
            ConnectDB();
            using (var cmd = new SQLiteCommand(conn)) {
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd, @$"SELECT * FROM {TableConst.tagsTName}"); // direct child only!
                while (reader.Read())
                    tags.Add(new Tag(Convert.ToInt32(reader[$"{TableConst.tagsCoID}"]), (string)reader[$"{TableConst.tagsCoName}"]));
            }
            DisconnectDB();
            return tags;
        }
        public List<string> GetAllTagsAsStrings() {
            List<string> tags = new();
            ConnectDB();
            using (var cmd = new SQLiteCommand(conn)) {
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd, @$"SELECT {TableConst.tagsCoName} FROM {TableConst.tagsTName}"); // direct child only!
                while (reader.Read())
                    tags.Add((string)reader[$"{TableConst.tagsCoName}"]);
            }
            DisconnectDB();
            return tags;
        }
        #endregion

        #region Rename Tag
        public int RenameTag(int tagID, string newName) {
            return GetAffectedRowsFromQueries($"UPDATE {TableConst.tagsTName} SET {TableConst.tagsCoName} = $newname" +
                $" WHERE {TableConst.tagsCoID} = {tagID}", new List<string>{"$newname"}, new List<string> { newName });
        }
        public int RenameTag(string tagName, string newName) { // won't be used... 
            if (tagName == newName)
                return 0; // otherwise 1 will be returned

            return GetAffectedRowsFromQueries($"UPDATE {TableConst.tagsTName} SET {TableConst.tagsCoName} = $newname" +
                $" WHERE {TableConst.tagsCoName} = $tagName", new List<string> { "$newname", "$tagName" },
                new List<string> { newName, tagName });
        }
        #endregion

        #region Delete Tag
        public int DeleteTag(int tagID) {
            return GetAffectedRowsFromQuery($"DELETE FROM {TableConst.tagsTName} WHERE tid = {tagID}");
        }
        #endregion

        #region Query functions
        public int GetAffectedRowsFromQuery(string cmdText) {
            int rowsAffected = 0;
            ConnectDB();
            using (var cmd = new SQLiteCommand(conn)) {
                rowsAffected = DBController.ExecuteNonQCommand(cmd, cmdText);
                cmd.Dispose();
            }
            DisconnectDB();
            return rowsAffected;
        }
        public int GetAffectedRowsFromQueries(string cmdText, List<string> paramName, List<string> paramValue) {
            int rowsAffected = 0;
            ConnectDB();
            using (var cmd = new SQLiteCommand(conn)) {
                for (int i = 0; i < paramName.Count; i++)
                    cmd.Parameters.AddWithValue(paramName[i], paramValue[i]);
                rowsAffected = DBController.ExecuteNonQCommand(cmd, cmdText);
                cmd.Dispose();
            }
            DisconnectDB();
            return rowsAffected;
        }
        #endregion



    }
}
